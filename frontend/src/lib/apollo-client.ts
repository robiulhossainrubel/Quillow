import { ApolloClient, InMemoryCache, HttpLink, split } from "@apollo/client";
import { setContext } from "@apollo/client/link/context";
import { GraphQLWsLink } from "@apollo/client/link/subscriptions";
import { createClient } from "graphql-ws";
import { getMainDefinition } from "@apollo/client/utilities";

const HTTP_URL = process.env.NEXT_PUBLIC_GRAPHQL_HTTP || "http://localhost:5049/graphql";
const WS_URL = process.env.NEXT_PUBLIC_GRAPHQL_WS || "ws://localhost:5049/graphql";

// 1. HTTP Link
const httpLink = new HttpLink({
  uri: HTTP_URL,
});

// 2. Auth Link — JWT Token dynamically attach করা
const authLink = setContext((_, { headers }) => {
  let token: string | null = null;
  if (typeof window !== "undefined") {
    token = localStorage.getItem("quillow_jwt");
  }

  return {
    headers: {
      ...headers,
      authorization: token ? `Bearer ${token}` : "",
    },
  };
});

// 3. WebSocket Link — Real-time Subscriptions এর জন্য
const wsLink =
  typeof window !== "undefined"
    ? new GraphQLWsLink(
        createClient({
          url: WS_URL,
          connectionParams: () => {
            const token = localStorage.getItem("quillow_jwt");
            return {
              authorization: token ? `Bearer ${token}` : "",
            };
          },
        })
      )
    : null;

// 4. Split Link — Subscription হলে WebSocket, না হলে HTTP
const splitLink =
  typeof window !== "undefined" && wsLink
    ? split(
        ({ query }) => {
          const definition = getMainDefinition(query);
          return (
            definition.kind === "OperationDefinition" &&
            definition.operation === "subscription"
          );
        },
        wsLink,
        authLink.concat(httpLink)
      )
    : authLink.concat(httpLink);

export const apolloClient = new ApolloClient({
  link: splitLink,
  cache: new InMemoryCache({
    typePolicies: {
      Query: {
        fields: {
          posts: {
            merge(existing, incoming) {
              return incoming;
            },
          },
        },
      },
    },
  }),
});
