export type Supplier = {
  id: string;
  code: string;
  name: string;
  email: string | null;
  phoneNumber: string | null;
  address: string | null;
  isActive: boolean;
  createdAtUtc: string;
  updatedAtUtc: string;
};

export type SupplierInput = {
  code: string;
  name: string;
  email?: string;
  phoneNumber?: string;
  address?: string;
};

export type SupplierListResult = {
  items: Supplier[];
  page: number;
  pageSize: number;
  totalCount: number;
};
