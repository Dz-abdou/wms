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
} as const;

export const administrationRoles = ["admin", "manager", "operator"] as const;

export const administratorRole = administrationRoles[0];
