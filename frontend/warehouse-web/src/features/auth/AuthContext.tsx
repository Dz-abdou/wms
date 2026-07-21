import {
  createContext,
  useContext,
  useEffect,
  useState,
  type PropsWithChildren,
} from "react";
import { configureAuthentication } from "../../shared/api/apiClient";
import {
  clearAccessToken,
  getAccessToken,
  getSession,
  login,
  logout,
  refreshAccessToken,
  type Session,
} from "./api/authApi";

type AuthContextValue = {
  isLoading: boolean;
  session: Session | null;
  signIn: (email: string, password: string) => Promise<void>;
  signOut: () => Promise<void>;
};
const AuthContext = createContext<AuthContextValue | undefined>(undefined);
export function AuthProvider({ children }: PropsWithChildren) {
  const [session, setSession] = useState<Session | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  useEffect(() => {
    configureAuthentication({
      getAccessToken,
      refreshAccessToken: async () => {
        try {
          await refreshAccessToken();
          return true;
        } catch {
          clearAccessToken();
          setSession(null);
          return false;
        }
      },
    });
    void (async () => {
      try {
        await refreshAccessToken();
        setSession(await getSession());
      } catch {
        clearAccessToken();
        setSession(null);
      } finally {
        setIsLoading(false);
      }
    })();
  }, []);
  async function signIn(email: string, password: string) {
    await login(email, password);
    setSession(await getSession());
  }
  async function signOut() {
    try {
      await logout();
    } finally {
      clearAccessToken();
      setSession(null);
    }
  }
  return (
    <AuthContext.Provider value={{ isLoading, session, signIn, signOut }}>
      {children}
    </AuthContext.Provider>
  );
}
export function useAuth() {
  const value = useContext(AuthContext);
  if (!value) throw new Error("useAuth must be used within AuthProvider.");
  return value;
}
