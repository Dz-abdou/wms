import { useLocation, useNavigate } from "react-router-dom";

type ReturnNavigationState = {
  returnTo?: unknown;
};

function isSafeInternalRoute(value: unknown): value is string {
  return typeof value === "string" && value.startsWith("/") && !value.startsWith("//");
}

export function locationTarget(location: {
  pathname: string;
  search: string;
}) {
  return `${location.pathname}${location.search}`;
}

export function useReturnDestination(fallback: string) {
  const location = useLocation();
  const navigate = useNavigate();
  const state = location.state as ReturnNavigationState | null;
  const returnTo = isSafeInternalRoute(state?.returnTo)
    ? state.returnTo
    : fallback;

  return {
    returnTo,
    returnState: { returnTo: locationTarget(location) },
    goBack: () => navigate(returnTo),
  };
}
