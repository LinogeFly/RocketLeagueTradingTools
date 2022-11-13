import React, {useEffect, useRef, useState} from "react";
import axios from 'axios';
import {NotificationDto} from "../../models/api/notification";
import {logError} from "../../services/logger";
import {config} from "../../services/config";
import {useQuery} from '@tanstack/react-query'
import {Link} from "react-router-dom";

// Page view model
interface Notification {
    id: number,
    itemName: string,
    itemPrice: number,
    tradeOfferAge: string,
    tradeOfferLink: string,
    status: NotificationStatus
}

enum NotificationStatus {
    New,
    Seen,
    PendingMarkAsSeen
}

const isNew = (n: Notification) => n.status === NotificationStatus.New;

const ensureNotificationPopupPermissionGranted = (): Promise<void> => {
    return new Promise<void>((resolve, reject) => {
        if (Notification.permission === 'granted')
            return resolve();

        Notification.requestPermission().then(function (p) {
            if (p === 'granted')
                return resolve();

            return reject('User blocked notification permission.');
        });
    });
}

const showNotificationPopup = () => {
    return new Notification("RLTT", { "body": "New trade offer alert." });
}

const playNotificationSound = (audioRef: React.RefObject<HTMLAudioElement>) => {
    if (!audioRef.current)
        return;

    audioRef.current.volume = config.notificationSoundEffectVolume;
    audioRef.current.play();
}

function NotificationsPage() {
    const [fetchingEnabled, setFetchingEnabled] = useState<boolean>(false);
    const [notifications, setNotifications] = useState<Notification[]>([]);
    const notificationAudioRef = useRef<HTMLAudioElement>(null);

    useEffect(() => {
        ensureNotificationPopupPermissionGranted()
            .then(() => setFetchingEnabled(true))
            .catch((reason) => console.error(reason));
    }, []);

    const { status } = useQuery(["notifications"], (context) => {
        return axios.get<NotificationDto[]>(`/api/notifications?pageSize=${config.notificationsPageSize}`, { signal: context.signal })
            .then(response => response.data);
    }, {
        enabled: fetchingEnabled,
        cacheTime: 0,
        refetchIntervalInBackground: true,
        refetchOnWindowFocus: false,
        refetchInterval: config.notificationsRefreshInterval,
        onSuccess: (data: NotificationDto[]) => {
            const newNotifications = data
                .filter(d => d.isNew)
                .filter(d => !notifications.some(c => c.id === d.id));

            if (newNotifications.length > 0) {
                showNotificationPopup();
                playNotificationSound(notificationAudioRef);
            }

            setNotifications(current => {
                const mapStatus = (notification: NotificationDto) => {
                    const status = current.find(n => n.id === notification.id)?.status;
                    
                    if (status === NotificationStatus.PendingMarkAsSeen)
                        return NotificationStatus.PendingMarkAsSeen;

                    return notification.isNew ? NotificationStatus.New : NotificationStatus.Seen;
                };
               
                return data.map(n => ({
                    id: n.id,
                    itemName: n.itemName,
                    itemPrice: n.itemPrice,
                    tradeOfferAge: n.tradeOfferAge,
                    tradeOfferLink: n.tradeOfferLink,
                    status: mapStatus(n)
                }));
            });
        },
        onError: () => {
            setNotifications([]);
        }
    });

    useEffect(() => {
        const newNotificationsCount = notifications.filter(n => isNew(n)).length;

        if (newNotificationsCount > 0)
            document.title = `(${newNotificationsCount}) ${config.defaultTitle}`;
        else
            document.title = config.defaultTitle;

    }, [notifications]);

    const hasNewNotifications = () => {
        return notifications.filter(n => isNew(n)).length > 0;
    }

    const setNotificationStatus = (id: number, status: NotificationStatus) => {
        setNotifications(current => 
            current.map(item => {
                if (item.id === id) {
                    return { ...item, status: status }
                }

                return item;
            })
        );
    }
    
    const setNotificationsStatus = (predicate: (n: Notification) => boolean, status: NotificationStatus) => {
        setNotifications(current =>
            current.map(item => {
                if (predicate(item)) {
                    return {...item, status: status}
                }

                return item;
            })
        );
    }

    const handleMarkAsSeenClick = (id: number) => {
        // Mark the notification as seen in the view model
        setNotificationStatus(id, NotificationStatus.PendingMarkAsSeen);

        // Send the update request to the back-end
        axios
            .patch(`/api/notifications/${id}`, { "markAsSeen": true })
            .then(() => setNotificationStatus(id, NotificationStatus.Seen))
            .catch(error => {
                if (axios.isCancel(error))
                    return;
                
                setNotificationStatus(id, NotificationStatus.New);
                logError(error);
            });
    }

    const handleMarkAllAsSeenClick = () => {
        // Mark all new notifications as seen in the view model
        setNotificationsStatus(n => n.status === NotificationStatus.New, NotificationStatus.PendingMarkAsSeen)
        
        // Send the update request to the back-end
        axios
            .post(`/api/notifications/mark-all-as-seen`)
            .then(() => setNotificationsStatus(n => n.status === NotificationStatus.PendingMarkAsSeen, NotificationStatus.Seen))
            .catch(error => {
                if (axios.isCancel(error))
                    return;

                setNotificationsStatus(n => n.status === NotificationStatus.PendingMarkAsSeen, NotificationStatus.New);
                logError(error);
            });
    }

    return (
        <>
            <div className="mb-3 d-flex align-items-end">
                <h2 className="m-0 flex-fill">Notifications</h2>
                {status === 'success' && notifications.length > 0 && <div>
                    <Link to=""
                          className={hasNewNotifications() ? '' : 'disabled'}
                          onClick={handleMarkAllAsSeenClick}>
                        Mark all as seen
                    </Link>
                </div>}
            </div>

            {status === 'loading' && <div>
                Loading...
            </div>}

            {status === 'error' && <div className="alert alert-danger" role="alert">
                Unable to load notifications. Try again later.
            </div>}

            <ol className="list-group">
                {notifications?.map(notification => {
                    const isNewClass = isNew(notification) ? 'list-group-item-primary' : '';

                    return (
                        <li className={`list-group-item ${isNewClass}`} key={notification.id}>
                            <div className="d-flex mb-2">
                                <div className="me-auto"><strong>{notification.itemName}</strong> for {notification.itemPrice} credits is matching a trade offer alert.</div>
                                <div className="ms-4"><small className="text-nowrap">{notification.tradeOfferAge} ago</small></div>
                            </div>
                            <div>
                                <a href={notification.tradeOfferLink} target="_blank" rel="noreferrer">Trade details</a>
                                <span className="px-2">•</span>
                                <Link to=""
                                    className={isNew(notification) ? '' : 'disabled'}
                                    onClick={() => handleMarkAsSeenClick(notification.id)}>
                                    Mark as seen
                                </Link>
                            </div>
                        </li>
                    )
                })}
            </ol>
            <audio ref={notificationAudioRef} src={config.notificationSoundEffectSource} preload="none"></audio>
        </>
    );
}

export default NotificationsPage;