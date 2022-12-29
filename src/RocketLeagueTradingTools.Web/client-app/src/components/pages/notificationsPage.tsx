import React, {useEffect, useRef, useState} from "react";
import axios from 'axios';
import {NotificationDto, NotificationsResponse} from "../../models/api/notification";
import {logError} from "../../services/logger";
import {config} from "../../services/config";
import {useQuery} from '@tanstack/react-query'
import {Link} from "react-router-dom";

// Page view model
interface Notifications {
    items: Notification[],
    total: number
}
//
interface Notification {
    id: number,
    itemName: string,
    itemPrice: number,
    tradeOfferAge: string,
    tradeOfferLink: string,
    status: NotificationStatus
}
//
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
    const [notifications, setNotifications] = useState<Notifications>({items: [], total: 0});
    const [numberOfPagesToShow, setNumberOfPagesToShow] = useState<number>(1);
    const [isLoadingMore, setIsLoadingMore] = useState<boolean>(false);
    const notificationAudioRef = useRef<HTMLAudioElement>(null);

    useEffect(() => {
        ensureNotificationPopupPermissionGranted()
            .then(() => setFetchingEnabled(true))
            .catch((reason) => console.error(reason));
    }, []);

    const { status } = useQuery(["notifications", numberOfPagesToShow], (context) => {
        const url = `/api/notifications?pageSize=${config.notificationsPageSize * numberOfPagesToShow}`;
        
        return axios
            .get<NotificationsResponse>(url, { signal: context.signal })
            .then(response => response.data);
    }, {
        enabled: fetchingEnabled,
        cacheTime: 0,
        refetchIntervalInBackground: true,
        refetchOnWindowFocus: false,
        refetchInterval: config.notificationsRefreshInterval,
        onSuccess: (data: NotificationsResponse) => {
            const newNotifications = data.items
                .filter(d => d.isNew)
                .filter(d => !notifications.items.some(c => c.id === d.id));

            if (newNotifications.length > 0) {
                showNotificationPopup();
                playNotificationSound(notificationAudioRef);
            }

            setNotifications(current => {
                const mapStatus = (notification: NotificationDto) => {
                    const status = current.items.find(n => n.id === notification.id)?.status;
                    
                    if (status === NotificationStatus.PendingMarkAsSeen)
                        return NotificationStatus.PendingMarkAsSeen;

                    return notification.isNew ? NotificationStatus.New : NotificationStatus.Seen;
                };
                
                return {
                    items: data.items.map(n => ({
                        id: n.id,
                        itemName: n.itemName,
                        itemPrice: n.itemPrice,
                        tradeOfferAge: n.tradeOfferAge,
                        tradeOfferLink: n.tradeOfferLink,
                        status: mapStatus(n)
                    })),
                    total: data.total
                }
            });
        },
        onError: () => {
            setNotifications({items: [], total: 0});
        },
        onSettled: () => {
            if (isLoadingMore) {
                setIsLoadingMore(false);
            }
        }
    });

    useEffect(() => {
        const newNotificationsCount = notifications.items.filter(n => isNew(n)).length;

        if (newNotificationsCount > 0)
            document.title = `(${newNotificationsCount}) ${config.defaultTitle}`;
        else
            document.title = config.defaultTitle;

    }, [notifications]);

    const hasNewNotifications = () => {
        return notifications.items.filter(n => isNew(n)).length > 0;
    }

    const setNotificationStatus = (id: number, status: NotificationStatus) => {
        setNotifications(current => ({
            items: current.items.map(item => {
                if (item.id === id) {
                    return {...item, status: status}
                }

                return item;
            }),
            total: current.total 
        }));
    }
    
    const setNotificationsStatus = (predicate: (n: Notification) => boolean, status: NotificationStatus) => {
        setNotifications(current => ({
            items: current.items.map(item => {
                if (predicate(item)) {
                    return {...item, status: status}
                }

                return item;
            }),
            total: current.total
        }));
    }
    
    const markNotificationAsSeen = (id: number) => {
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

    const handleMarkAsSeenClick = (event: React.MouseEvent, id: number) => {
        // Not letting the event to be bubbled to the notification item as it's clickable as well.
        event.stopPropagation();

        markNotificationAsSeen(id);
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
    
    const handleNotificationClick = (notification: Notification) => {
        markNotificationAsSeen(notification.id);
        window.open(notification.tradeOfferLink, "_blank", "noreferrer");
    }

    const handleLoadMoreClick = () => {
        setNumberOfPagesToShow(current => {
            return current + 1;
        });
        
        setIsLoadingMore(true);
    }
    
    const shouldShowMarkAllAsSeen = ():boolean => {
        if (notifications.items.length === 0)
            return false;
        
        return status === 'success' || isLoadingMore;
    }
    
    return (
        <>
            <div className="mb-3 d-flex align-items-end">
                <h2 className="m-0 flex-fill">Notifications</h2>
                {shouldShowMarkAllAsSeen() && <div>
                    <Link to=""
                          className={hasNewNotifications() ? '' : 'disabled'}
                          onClick={handleMarkAllAsSeenClick}>
                        Mark all as seen
                    </Link>
                </div>}
            </div>

            {status === 'loading' && !isLoadingMore && <div>
                Loading...
            </div>}

            {status === 'error' && <div className="alert alert-danger" role="alert">
                Unable to load notifications. Try again later.
            </div>}

            {status === 'success' && !notifications.items.length && <div>
                Nothing to see here.
            </div>}

            {notifications.items.map(notification => {
                const isNewClass = isNew(notification) ? 'rltt-notification--new' : '';

                return (
                    <div className={`rltt-notification ${isNewClass}`}
                         key={notification.id}
                         onClick={() => handleNotificationClick(notification)}>
                        <div className="rltt-notification__content">
                            <div className="rltt-notification__text">
                                <strong>{notification.itemName}</strong> for {notification.itemPrice} credits is matching a trade offer alert.
                            </div>
                            
                            <div className="rltt-notification__footer">
                                <div className="rltt-notification__age">{notification.tradeOfferAge} ago</div>
                                {isNew(notification) && <div className="rltt-notification__mark-as-seen">
                                     <Link to=""
                                          onClick={(e) => handleMarkAsSeenClick(e, notification.id)}>
                                        Mark as seen
                                    </Link>
                                </div>}
                            </div>
                        </div>
                    </div>
                )
            })}

            {notifications.total > notifications.items.length && <div className="my-3 primary text-center">
                {isLoadingMore
                    ? <>Loading...</>
                    : <Link to="" onClick={handleLoadMoreClick}>Show more</Link>}                    
            </div>}
            
            <audio ref={notificationAudioRef} src={config.notificationSoundEffectSource} preload="none"></audio>
        </>
    );
}

export default NotificationsPage;