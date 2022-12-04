export interface NotificationsResponse {
    total: number,
    items: NotificationDto[]
}

export interface NotificationDto {
    id: number,
    itemName: string,
    itemPrice: number,
    tradeOfferAge: string,
    tradeOfferLink: string,
    isNew: boolean
}
