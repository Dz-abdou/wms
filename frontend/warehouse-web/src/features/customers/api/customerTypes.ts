export type Customer = {
  id: string;
  code: string;
  legalName: string;
  tradingName: string | null;
  defaultCurrencyCode: string | null;
  deliveryInstructions: string | null;
  serviceNotes: string | null;
  isActive: boolean;
  createdAtUtc: string;
  updatedAtUtc: string;
  contacts: CustomerContact[];
  addresses: CustomerAddress[];
};

export type CustomerListItem = Pick<
  Customer,
  | "id"
  | "code"
  | "legalName"
  | "tradingName"
  | "defaultCurrencyCode"
  | "isActive"
>;

export type CustomerInput = {
  code: string;
  legalName: string;
  tradingName?: string;
  defaultCurrencyCode?: string;
  deliveryInstructions?: string;
  serviceNotes?: string;
};

export type CustomerContact = {
  id: string;
  name: string;
  role: string | null;
  email: string | null;
  phoneNumber: string | null;
};

export type CustomerContactInput = Omit<CustomerContact, "id">;

export type CustomerAddress = {
  id: string;
  label: string;
  addressLine1: string;
  addressLine2: string | null;
  city: string;
  postalCode: string | null;
  countryCode: string;
  isShippingAddress: boolean;
  isBillingAddress: boolean;
  deliveryInstructions: string | null;
};

export type CustomerAddressInput = Omit<CustomerAddress, "id">;

export type CustomerListResult = {
  items: CustomerListItem[];
  page: number;
  pageSize: number;
  totalCount: number;
};

export type CustomerListQuery = {
  page: number;
  pageSize: number;
  search?: string;
  isActive?: boolean;
};
