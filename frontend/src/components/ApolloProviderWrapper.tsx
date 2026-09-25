"use client";

import React from "react";
import { ApolloProvider } from "@apollo/client/react";
import { apolloClient } from "@/lib/apollo-client";
import { AuthProvider } from "@/context/AuthContext";

export function ApolloProviderWrapper({ children }: { children: React.ReactNode }) {
  return (
    <ApolloProvider client={apolloClient}>
      <AuthProvider>{children}</AuthProvider>
    </ApolloProvider>
  );
}
