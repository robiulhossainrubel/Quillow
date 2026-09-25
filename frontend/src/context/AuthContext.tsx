"use client";

import React, { createContext, useContext, useEffect, useState } from "react";
import { useLazyQuery, useMutation } from "@apollo/client/react";
import { GET_ME, LOGIN_MUTATION, REGISTER_MUTATION } from "@/graphql/operations";

export interface UserProfile {
  id: string;
  displayName: string;
  email: string;
  avatar?: string;
  bio?: string;
  roles?: string[];
  postCount?: number;
  commentCount?: number;
}

interface AuthContextType {
  user: UserProfile | null;
  token: string | null;
  loading: boolean;
  login: (email: string, password: string) => Promise<{ success: boolean; error?: string }>;
  register: (
    userName: string,
    email: string,
    password: string,
    displayName: string
  ) => Promise<{ success: boolean; error?: string }>;
  logout: () => void;
  isAuthModalOpen: boolean;
  setIsAuthModalOpen: (open: boolean) => void;
  authModalMode: "login" | "register";
  setAuthModalMode: (mode: "login" | "register") => void;
  openLoginModal: () => void;
  openRegisterModal: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<UserProfile | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [isAuthModalOpen, setIsAuthModalOpen] = useState(false);
  const [authModalMode, setAuthModalMode] = useState<"login" | "register">("login");

  const [fetchMe] = useLazyQuery<{ me: UserProfile }>(GET_ME, {
    fetchPolicy: "network-only",
  });

  const [loginMutation] = useMutation<{ login: { token?: string; user?: UserProfile; errors?: { message: string }[] } }>(LOGIN_MUTATION);
  const [registerMutation] = useMutation<{ register: { token?: string; user?: UserProfile; errors?: { message: string }[] } }>(REGISTER_MUTATION);

  useEffect(() => {
    const checkAuth = async () => {
      const savedToken = localStorage.getItem("quillow_jwt");
      if (savedToken) {
        setToken(savedToken);
        try {
          const res = await fetchMe();
          if (res.data?.me) {
            setUser(res.data.me);
          } else {
            logout();
          }
        } catch {
          logout();
        }
      }
      setLoading(false);
    };
    checkAuth();
  }, [fetchMe]);

  const login = async (email: string, password: string) => {
    try {
      const res = await loginMutation({
        variables: { input: { email, password } },
      });

      const payload = res.data?.login;
      if (payload?.token) {
        localStorage.setItem("quillow_jwt", payload.token);
        setToken(payload.token);
        if (payload.user) setUser(payload.user);
        setIsAuthModalOpen(false);
        const meRes = await fetchMe();
        if (meRes.data?.me) setUser(meRes.data.me);
        return { success: true };
      } else if (payload?.errors?.length) {
        return { success: false, error: payload.errors[0].message };
      }
      return { success: false, error: "Login failed" };
    } catch (err: unknown) {
      const errorMsg = err instanceof Error ? err.message : "Failed to connect to server";
      return { success: false, error: errorMsg };
    }
  };

  const register = async (
    userName: string,
    email: string,
    password: string,
    displayName: string
  ) => {
    try {
      const res = await registerMutation({
        variables: {
          input: { userName, email, password, displayName },
        },
      });

      const payload = res.data?.register;
      if (payload?.token) {
        localStorage.setItem("quillow_jwt", payload.token);
        setToken(payload.token);
        if (payload.user) setUser(payload.user);
        setIsAuthModalOpen(false);
        const meRes = await fetchMe();
        if (meRes.data?.me) setUser(meRes.data.me);
        return { success: true };
      } else if (payload?.errors?.length) {
        return { success: false, error: payload.errors[0].message };
      }
      return { success: false, error: "Registration failed" };
    } catch (err: unknown) {
      const errorMsg = err instanceof Error ? err.message : "Failed to connect to server";
      return { success: false, error: errorMsg };
    }
  };

  const logout = () => {
    localStorage.removeItem("quillow_jwt");
    setToken(null);
    setUser(null);
  };

  const openLoginModal = () => {
    setAuthModalMode("login");
    setIsAuthModalOpen(true);
  };

  const openRegisterModal = () => {
    setAuthModalMode("register");
    setIsAuthModalOpen(true);
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        token,
        loading,
        login,
        register,
        logout,
        isAuthModalOpen,
        setIsAuthModalOpen,
        authModalMode,
        setAuthModalMode,
        openLoginModal,
        openRegisterModal,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return context;
};
