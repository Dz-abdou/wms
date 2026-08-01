export const customerApiPaths = {
  base: "/api/customers",
  byId: (id: string) => `/api/customers/${id}`,
  status: (id: string) => `/api/customers/${id}/status`,
  contacts: (id: string) => `/api/customers/${id}/contacts`,
  contact: (id: string, contactId: string) =>
    `/api/customers/${id}/contacts/${contactId}`,
  addresses: (id: string) => `/api/customers/${id}/addresses`,
  address: (id: string, addressId: string) =>
    `/api/customers/${id}/addresses/${addressId}`,
} as const;

export const customerRoutes = {
  list: "/customers",
  listPattern: "customers",
  create: "/customers/new",
  detail: (id: string) => `/customers/${id}`,
  detailPattern: "customers/:id",
  edit: (id: string) => `/customers/${id}/edit`,
  editPattern: "customers/:id/edit",
} as const;

export const customerValidation = {
  maxCodeLength: 32,
  maxLegalNameLength: 200,
  maxTradingNameLength: 200,
  maxDeliveryInstructionsLength: 1000,
  maxServiceNotesLength: 1000,
  maxContactNameLength: 200,
  maxContactRoleLength: 100,
  maxEmailLength: 320,
  maxPhoneNumberLength: 50,
  maxAddressLabelLength: 100,
  maxAddressLineLength: 200,
  maxCityLength: 100,
  maxPostalCodeLength: 32,
  maxAddressInstructionsLength: 1000,
} as const;
