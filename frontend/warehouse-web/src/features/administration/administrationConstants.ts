export const administrationApiPaths = {
  roles: "/api/admin/roles",
  users: "/api/admin/users",
  userRoles: (id: string) => `/api/admin/users/${id}/roles`,
  user: (id: string) => `/api/admin/users/${id}`,
} as const;

export const administrationRoutes = {
  roles: "/administration/roles",
  rolesPattern: "administration/roles",
  users: "/administration/users",
  usersPattern: "administration/users",
  userCreate: "/administration/users/new",
  userCreatePattern: "administration/users/new",
  userEdit: (id: string) => `/administration/users/${id}/edit`,
  userEditPattern: "administration/users/:id/edit",
} as const;

export const administrationRoles = ["admin", "manager", "operator"] as const;

export const administratorRole = administrationRoles[0];
