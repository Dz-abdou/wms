export type AdministrationUser = {
  id: string;
  email: string;
  roles: string[];
};

export type CreateUserValues = {
  email: string;
  password: string;
};

export type UpdateUserValues = {
  email: string;
};

export type AdministrationUserListQuery = {
  page: number;
  pageSize: number;
  email?: string;
  role?: string;
};

export type AdministrationUserListResult = {
  items: AdministrationUser[];
  page: number;
  pageSize: number;
  totalCount: number;
};
