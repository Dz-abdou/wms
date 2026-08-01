export const inventoryApiPaths = {
  adjustments: "/api/inventory/adjustments",
  movements: "/api/inventory/movements",
  overview: "/api/inventory/overview",
  cycleCounts: "/api/inventory/cycle-counts",
  cycleCountCandidate: "/api/inventory/cycle-counts/candidate",
  transfers: "/api/inventory/transfers",
  transferCandidate: "/api/inventory/transfers/candidate",
} as const;

export const inventoryRoutes = {
  root: "/inventory",
  rootPattern: "inventory",
  overview: "/inventory/overview",
  overviewPattern: "inventory/overview",
  movementHistory: "/inventory/movements",
  movementHistoryPattern: "inventory/movements",
  adjustments: "/inventory/adjustments",
  adjustmentsPattern: "inventory/adjustments",
  adjustmentCreate: "/inventory/adjustments/new",
  adjustmentCreatePattern: "inventory/adjustments/new",
  adjustmentDetail: (id: string) => `/inventory/adjustments/${id}`,
  adjustmentDetailPattern: "inventory/adjustments/:id",
  cycleCounts: "/inventory/cycle-counts",
  cycleCountsPattern: "inventory/cycle-counts",
  cycleCountCreate: "/inventory/cycle-counts/new",
  cycleCountCreatePattern: "inventory/cycle-counts/new",
  cycleCountDetail: (id: string) => `/inventory/cycle-counts/${id}`,
  cycleCountDetailPattern: "inventory/cycle-counts/:id",
  transfers: "/inventory/transfers",
  transfersPattern: "inventory/transfers",
  transferCreate: "/inventory/transfers/new",
  transferCreatePattern: "inventory/transfers/new",
  transferDetail: (id: string) => `/inventory/transfers/${id}`,
  transferDetailPattern: "inventory/transfers/:id",
} as const;

export const inventoryPageSize = 100;

export const fractionalBaseUnitCodes: ReadonlySet<string> = new Set([
  "KG",
  "G",
  "L",
  "ML",
  "M",
  "CM",
  "MM",
]);
