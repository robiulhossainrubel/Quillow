import type { Metadata } from "next";
import "./globals.css";
import { ApolloProviderWrapper } from "@/components/ApolloProviderWrapper";

export const metadata: Metadata = {
  title: "Quillow — Fullstack GraphQL Blog & CMS (.NET 10 + Next.js 15)",
  description:
    "Production-grade GraphQL Blog Platform with Hot Chocolate .NET 10, Apollo Client 3, DataLoaders, and Real-time WebSockets.",
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en">
      <body>
        <ApolloProviderWrapper>{children}</ApolloProviderWrapper>
      </body>
    </html>
  );
}
