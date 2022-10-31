import { useEffect, useState, useRef } from "react";
import axios from 'axios';
import { NotificationDto } from "../../models/api/notification";
import { logError } from "../../services/logger";
import { config } from "../../services/config";
import { useQuery } from '@tanstack/react-query'
import { Link } from "react-router-dom";

interface NotificationViewModel {
    id: number,
    itemName: string,
    itemPrice: number,
    tradeOfferAge: string,
    tradeOfferLink: string,
    isNew: boolean
}

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
    return new Notification("RLTT", { "body": "New trade item alert." });
}

const playNotificationSound = (audioRef: React.RefObject<HTMLAudioElement>) => {
    if (!audioRef.current)
        return;

    audioRef.current.volume = config.notificationSoundEffectVolume;
    audioRef.current.play();
}

function NotificationsPage() {
    const [fetchingEnabled, setFetchingEnabled] = useState<boolean>(false);
    const [notifications, setNotifications] = useState<NotificationViewModel[]>([]);
    const notificationAudioRef = useRef<HTMLAudioElement>(null);

    useEffect(() => {
        ensureNotificationPopupPermissionGranted()
            .then(() => setFetchingEnabled(true))
            .catch((reason) => console.error(reason));
    }, []);

    const { status } = useQuery(["notifications"], (context) => {
        return axios.get<NotificationDto[]>('/api/notifications', { signal: context.signal })
            .then(response => response.data);
    }, {
        enabled: fetchingEnabled,
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

            setNotifications(data.map(n => ({
                id: n.id,
                itemName: n.itemName,
                itemPrice: n.itemPrice,
                tradeOfferAge: n.tradeOfferAge,
                tradeOfferLink: n.tradeOfferLink,
                isNew: n.isNew
            })));
        },
        onError: () => {
            setNotifications([]);
        }
    });

    useEffect(() => {
        const newNotificationsCount = notifications.filter(n => n.isNew).length;

        if (newNotificationsCount > 0)
            document.title = `(${newNotificationsCount}) ${config.defaultTitle}`;
        else
            document.title = config.defaultTitle;

    }, [notifications]);

    const handleMarkAsSeenClick = (id: number) => {
        // Mark notification as seen in the view model
        setNotifications(current => {
            const newItems: NotificationViewModel[] = current.map(item => {
                if (item.id === id) {
                    return { ...item, isNew: false }
                }

                return item;
            });

            return newItems;
        });

        // Send the update request to the back-end
        axios
            .patch(`/api/notifications/${id}`, { "markAsSeen": true })
            .catch(error => {
                if (axios.isCancel(error))
                    return;

                logError(error);
            });
    }

    return (
        <>
            <h2>Notifications</h2>

            {status === 'loading' && <div>
                Loading...
            </div>}

            {status === 'error' && <div className="alert alert-danger" role="alert">
                Unable to load notifications. Try again later.
            </div>}

            <ol className="list-group">
                {notifications?.map(notification => {
                    const isNewClass = notification.isNew ? 'list-group-item-primary' : '';

                    return (
                        <li className={`list-group-item ${isNewClass}`} key={notification.id}>
                            <div className="d-flex mb-2">
                                <div className="me-auto"><strong>{notification.itemName}</strong> for {notification.itemPrice} credits is matching a trade item alert.</div>
                                <div className="ms-4"><small className="text-nowrap">{notification.tradeOfferAge} ago</small></div>
                            </div>
                            <div>
                                <a href={notification.tradeOfferLink} target="_blank" rel="noreferrer">Trade details</a>
                                <span className="px-2">•</span>
                                <Link to=""
                                    className={notification.isNew ? '' : 'disabled'}
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