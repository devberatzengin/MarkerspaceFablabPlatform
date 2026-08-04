export type UserType = 'Unknown' | 'Admin' | 'User' | 'Staff';
export type MembershipStatus = 'Unknown' | 'Free' | 'Bronze' | 'Silver' | 'Gold' | 'Professional';
export type EquipmentType = 'Unknown' | 'DigitalFabrication' | 'HandTools' | 'PowerTools' | 'Electronics' | 'Measurement';
export type EquipmentStatus = 'Unknown' | 'Available' | 'Reserved' | 'Rented' | 'Maintenance';
export type EquipmentPlacementType = 'Unknown' | 'Portable' | 'Benchtop' | 'FloorStationary';
export type CategoryType = 'Undefined' | 'Draft' | 'Published' | 'Unpublished' | 'Archived';
export type ContentStatus = 'Draft' | 'Published' | 'Unpublished' | 'Archived';
export type PaymentStatus = 'Pending' | 'Paid' | 'Failed' | 'Refunded' | 'Cancelled';
export type PaymentMethod =
  | 'Balance'
  | 'Cash'
  | 'CreditCard'
  | 'DebitCard'
  | 'BankTransfer'
  | 'Stripe'
  | 'PayPal'
  | 'Other';

export interface AuthResponse {
  token: string;
  email: string;
  type: UserType;
}

export interface RegisterRequest {
  username: string;
  password: string;
  email: string;
  firstName: string;
  lastName: string;
  phoneNumber: string;
}

export interface LoginRequest {
  email: string;
  password: string;
  userName?: string;
}

export interface UserResponse {
  id: string;
  username: string;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  type: UserType;
  equipmentLevel: number;
  balance: number;
  status: MembershipStatus;
  isActive: boolean;
  createdAt: string;
}

export interface UserUpdateRequest {
  firstName?: string;
  lastName?: string;
  email?: string;
  phoneNumber?: string;
  status: MembershipStatus;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

export interface CategoryResponse {
  id: string;
  name: string;
  type: CategoryType;
  isActive: boolean;
}

export interface CategoryCreateRequest {
  name: string;
  type: CategoryType;
}

export interface CategoryUpdateRequest {
  id: string;
  name: string;
  type: CategoryType;
  isActive: boolean;
}

export interface AnnouncementResponse {
  id: string;
  title: string;
  content: string;
  createdByUserId: string;
  createdByName: string;
  categoryName: string;
  categoryId: string;
  status: ContentStatus;
  createdAt: string;
  updatedAt: string;
}

export interface AnnouncementCreateRequest {
  title: string;
  content: string;
  categoryId: string;
}

export interface AnnouncementUpdateRequest {
  id: string;
  title: string;
  content: string;
  categoryId: string;
}

export interface EquipmentResponse {
  id: string;
  name: string;
  description: string;
  type: EquipmentType;
  placementType: EquipmentPlacementType;
  status: EquipmentStatus;
  requiredUserLevel: number;
  isDeleted: boolean;
  currentUserId?: string;
  availableAt: string;
}

export interface EquipmentCreateRequest {
  name: string;
  description: string;
  type: EquipmentType;
  placementType: EquipmentPlacementType;
  requiredUserLevel: number;
}

export interface EquipmentUpdateRequest {
  id: string;
  name?: string;
  status?: EquipmentStatus;
  usageTime?: string;
  requiredUserLevel?: number;
}

/** İleri tarihli rezervasyon isteği. Tarihler ISO-8601 UTC ("...Z") olarak gönderilir. */
export interface ReserveAheadRequest {
  startAt: string;
  endAt: string;
}

export interface EquipmentRentalResponse {
  id: string;
  userId: string;
  equipmentId: string;
  equipmentName: string;
  equipmentDescription: string;
  rentedAt: string;
  releasedAt?: string;
  expectedReturnAt: string;
  isActive: boolean;
}

export interface PaymentResponse {
  id: string;
  paymentNumber: string;
  equipmentRentalId: string;
  userId: string;
  rentalFee: number;
  lateFee: number;
  totalAmount: number;
  discountAmount: number;
  paymentMethod: PaymentMethod;
  status: PaymentStatus;
  createdAt: string;
  paidAt?: string;
}

/** Teslim edilmiş ama ödenmemiş bir kiralamanın tutar önizlemesi. */
export interface PendingPaymentResponse {
  equipmentRentalId: string;
  equipmentId: string;
  equipmentName: string;
  rentedAt: string;
  expectedReturnAt: string;
  releasedAt: string;
  isOverdue: boolean;
  overdueBy?: string;
  rentalFee: number;
  lateFee: number;
  discountAmount: number;
  totalAmount: number;
  userBalance: number;
  hasSufficientBalance: boolean;
}

export interface PaymentCreateRequest {
  equipmentRentalId: string;
  userId?: string;
}

export type SubscriptionTargetType = 'Category' | 'Equipment';
export type NotificationChannelType = 'InApp' | 'Email';
export type NotificationType =
  | 'AnnouncementPublished'
  | 'EquipmentMaintenance'
  | 'EquipmentAvailable'
  | 'RentalEndsSoon';

export interface SubscriptionResponse {
  id: string;
  targetType: SubscriptionTargetType;
  categoryId?: string;
  categoryName?: string;
  equipmentId?: string;
  equipmentName?: string;
  channel: NotificationChannelType;
  isActive: boolean;
  createdAt: string;
}

export interface SubscriptionCreateRequest {
  targetType: SubscriptionTargetType;
  categoryId?: string;
  equipmentId?: string;
  channel: NotificationChannelType;
}

export interface NotificationResponse {
  id: string;
  type: NotificationType;
  channel: NotificationChannelType;
  title: string;
  message: string;
  relatedEntityId?: string;
  isRead: boolean;
  readAt?: string;
  createdAt: string;
}

export interface PagedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}
