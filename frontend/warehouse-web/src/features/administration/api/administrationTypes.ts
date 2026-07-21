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
