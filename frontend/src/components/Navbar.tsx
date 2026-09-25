"use client";

import React, { useState } from "react";
import { useAuth } from "@/context/AuthContext";
import {
  Feather,
  PlusCircle,
  Search,
  LogIn,
  LogOut,
  User,
  Radio,
  X,
  BookOpen,
  Folder,
} from "lucide-react";
import { useLazyQuery } from "@apollo/client/react";
import { GLOBAL_SEARCH } from "@/graphql/operations";

interface NavbarProps {
  onOpenCreatePost: () => void;
  onSelectPostBySlug?: (slug: string) => void;
}

interface SearchPost {
  __typename: "Post";
  id: string;
  title: string;
  slug: string;
  excerpt?: string;
  author?: { displayName: string };
}

interface SearchUser {
  __typename: "User";
  id: string;
  displayName: string;
  email: string;
}

interface SearchCategory {
  __typename: "Category";
  id: string;
  name: string;
  slug: string;
}

type SearchResultItem = SearchPost | SearchUser | SearchCategory;

export function Navbar({ onOpenCreatePost, onSelectPostBySlug }: NavbarProps) {
  const { user, logout, openLoginModal } = useAuth();
  const [searchTerm, setSearchTerm] = useState("");
  const [isSearchOpen, setIsSearchOpen] = useState(false);

  const [executeSearch, { data: searchData, loading: searching }] = useLazyQuery<{
    search: SearchResultItem[];
  }>(GLOBAL_SEARCH);

  const handleSearchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const val = e.target.value;
    setSearchTerm(val);
    if (val.trim().length >= 2) {
      executeSearch({ variables: { term: val.trim() } });
    }
  };

  const searchResults: SearchResultItem[] = searchData?.search || [];

  return (
    <>
      <header
        style={{
          position: "sticky",
          top: 0,
          zIndex: 100,
          background: "rgba(7, 9, 14, 0.8)",
          backdropFilter: "blur(18px)",
          borderBottom: "1px solid var(--card-border)",
        }}
      >
        <div
          style={{
            maxWidth: "1280px",
            margin: "0 auto",
            padding: "14px 24px",
            display: "flex",
            alignItems: "center",
            justifyContent: "space-between",
            gap: "20px",
          }}
        >
          {/* Logo */}
          <div
            style={{
              display: "flex",
              alignItems: "center",
              gap: "12px",
              cursor: "pointer",
            }}
            onClick={() => window.scrollTo({ top: 0, behavior: "smooth" })}
          >
            <div
              style={{
                width: "40px",
                height: "40px",
                borderRadius: "var(--radius-md)",
                background: "var(--accent-gradient)",
                display: "flex",
                alignItems: "center",
                justifyContent: "center",
                boxShadow: "var(--shadow-glow)",
              }}
            >
              <Feather size={22} color="#fff" />
            </div>
            <div>
              <span
                style={{
                  fontSize: "1.45rem",
                  fontWeight: 800,
                  letterSpacing: "-0.03em",
                }}
                className="gradient-text"
              >
                Quillow
              </span>
              <span
                style={{
                  fontSize: "0.7rem",
                  display: "block",
                  color: "var(--text-faint)",
                  fontWeight: 600,
                  letterSpacing: "0.08em",
                  marginTop: "-3px",
                }}
              >
                GRAPHQL CMS
              </span>
            </div>
          </div>

          {/* Search Trigger Button */}
          <div
            onClick={() => setIsSearchOpen(true)}
            style={{
              flex: "0 1 360px",
              display: "flex",
              alignItems: "center",
              gap: "10px",
              background: "rgba(255, 255, 255, 0.04)",
              border: "1px solid var(--card-border)",
              borderRadius: "var(--radius-full)",
              padding: "8px 16px",
              color: "var(--text-faint)",
              fontSize: "0.875rem",
              cursor: "pointer",
              transition: "all 0.2s ease",
            }}
          >
            <Search size={16} />
            <span>Search posts, authors, topics...</span>
            <kbd
              style={{
                marginLeft: "auto",
                background: "rgba(255, 255, 255, 0.08)",
                padding: "2px 7px",
                borderRadius: "4px",
                fontSize: "0.7rem",
              }}
            >
              /
            </kbd>
          </div>

          {/* Right Action Buttons */}
          <div style={{ display: "flex", alignItems: "center", gap: "14px" }}>
            {/* Live WebSocket indicator */}
            <div
              style={{
                display: "flex",
                alignItems: "center",
                gap: "8px",
                background: "rgba(16, 185, 129, 0.08)",
                border: "1px solid rgba(16, 185, 129, 0.2)",
                padding: "6px 12px",
                borderRadius: "var(--radius-full)",
                fontSize: "0.75rem",
                color: "var(--emerald)",
                fontWeight: 600,
              }}
              title="WebSocket Subscriptions connected to Hot Chocolate"
            >
              <div className="live-dot" />
              <span>LIVE</span>
            </div>

            {/* Create Post Button */}
            <button
              className="btn-primary"
              onClick={user ? onOpenCreatePost : openLoginModal}
              style={{ padding: "8px 16px", fontSize: "0.875rem" }}
            >
              <PlusCircle size={17} />
              <span>Write Post</span>
            </button>

            {/* User Profile / Login */}
            {user ? (
              <div
                style={{
                  display: "flex",
                  alignItems: "center",
                  gap: "10px",
                  padding: "4px 8px 4px 12px",
                  background: "rgba(255, 255, 255, 0.05)",
                  border: "1px solid var(--card-border)",
                  borderRadius: "var(--radius-full)",
                }}
              >
                <div style={{ textAlign: "right", lineHeight: 1.2 }}>
                  <div
                    style={{
                      fontSize: "0.85rem",
                      fontWeight: 700,
                      color: "var(--text-main)",
                    }}
                  >
                    {user.displayName}
                  </div>
                  <div
                    style={{
                      fontSize: "0.7rem",
                      color: "var(--accent-primary)",
                      fontWeight: 600,
                    }}
                  >
                    {user.roles?.[0] || "Author"}
                  </div>
                </div>
                <button
                  className="btn-icon"
                  onClick={logout}
                  title="Logout"
                  style={{ width: "32px", height: "32px" }}
                >
                  <LogOut size={15} />
                </button>
              </div>
            ) : (
              <button
                className="btn-secondary"
                onClick={openLoginModal}
                style={{ padding: "8px 16px", fontSize: "0.875rem" }}
              >
                <LogIn size={16} />
                <span>Sign In</span>
              </button>
            )}
          </div>
        </div>
      </header>

      {/* Global Search Modal (Demonstrates GraphQL Union Type) */}
      {isSearchOpen && (
        <div
          className="modal-overlay"
          onClick={() => setIsSearchOpen(false)}
        >
          <div
            className="modal-content"
            onClick={(e) => e.stopPropagation()}
            style={{ maxWidth: "640px" }}
          >
            <div
              style={{
                display: "flex",
                alignItems: "center",
                gap: "12px",
                padding: "16px 20px",
                borderBottom: "1px solid var(--card-border)",
              }}
            >
              <Search size={20} color="var(--accent-primary)" />
              <input
                type="text"
                autoFocus
                placeholder="Search across Posts, Authors, and Categories (Union Type)..."
                value={searchTerm}
                onChange={handleSearchChange}
                style={{
                  background: "transparent",
                  border: "none",
                  outline: "none",
                  color: "#fff",
                  fontSize: "1.05rem",
                  flex: 1,
                }}
              />
              <button
                className="btn-icon"
                onClick={() => setIsSearchOpen(false)}
                style={{ width: "32px", height: "32px" }}
              >
                <X size={16} />
              </button>
            </div>

            {/* Results */}
            <div style={{ maxHeight: "380px", overflowY: "auto", padding: "12px" }}>
              {searching && (
                <div
                  style={{
                    padding: "30px",
                    textAlign: "center",
                    color: "var(--text-faint)",
                  }}
                >
                  Querying Hot Chocolate Union Resolver...
                </div>
              )}

              {!searching && searchTerm.length >= 2 && searchResults.length === 0 && (
                <div
                  style={{
                    padding: "30px",
                    textAlign: "center",
                    color: "var(--text-faint)",
                  }}
                >
                  No results found for &quot;{searchTerm}&quot;
                </div>
              )}

              {!searching &&
                searchResults.map((item, idx) => {
                  if (item.__typename === "Post") {
                    return (
                      <div
                        key={idx}
                        onClick={() => {
                          setIsSearchOpen(false);
                          if (onSelectPostBySlug) onSelectPostBySlug(item.slug);
                        }}
                        style={{
                          padding: "12px 16px",
                          borderRadius: "var(--radius-md)",
                          background: "rgba(255, 255, 255, 0.03)",
                          marginBottom: "8px",
                          cursor: "pointer",
                          display: "flex",
                          alignItems: "center",
                          gap: "12px",
                          transition: "background 0.2s ease",
                        }}
                        onMouseEnter={(e) =>
                          (e.currentTarget.style.background =
                            "rgba(139, 92, 246, 0.15)")
                        }
                        onMouseLeave={(e) =>
                          (e.currentTarget.style.background =
                            "rgba(255, 255, 255, 0.03)")
                        }
                      >
                        <BookOpen size={18} color="var(--accent-primary)" />
                        <div style={{ flex: 1 }}>
                          <div style={{ fontWeight: 600, fontSize: "0.95rem" }}>
                            {item.title}
                          </div>
                          <div
                            style={{
                              fontSize: "0.75rem",
                              color: "var(--text-faint)",
                            }}
                          >
                            By {item.author?.displayName}
                          </div>
                        </div>
                        <span className="badge badge-purple">Post</span>
                      </div>
                    );
                  }

                  if (item.__typename === "User") {
                    return (
                      <div
                        key={idx}
                        style={{
                          padding: "12px 16px",
                          borderRadius: "var(--radius-md)",
                          background: "rgba(255, 255, 255, 0.03)",
                          marginBottom: "8px",
                          display: "flex",
                          alignItems: "center",
                          gap: "12px",
                        }}
                      >
                        <User size={18} color="var(--emerald)" />
                        <div style={{ flex: 1 }}>
                          <div style={{ fontWeight: 600, fontSize: "0.95rem" }}>
                            {item.displayName}
                          </div>
                          <div
                            style={{
                              fontSize: "0.75rem",
                              color: "var(--text-faint)",
                            }}
                          >
                            {item.email}
                          </div>
                        </div>
                        <span className="badge badge-emerald">Author</span>
                      </div>
                    );
                  }

                  if (item.__typename === "Category") {
                    return (
                      <div
                        key={idx}
                        style={{
                          padding: "12px 16px",
                          borderRadius: "var(--radius-md)",
                          background: "rgba(255, 255, 255, 0.03)",
                          marginBottom: "8px",
                          display: "flex",
                          alignItems: "center",
                          gap: "12px",
                        }}
                      >
                        <Folder size={18} color="var(--amber)" />
                        <div style={{ flex: 1 }}>
                          <div style={{ fontWeight: 600, fontSize: "0.95rem" }}>
                            {item.name}
                          </div>
                        </div>
                        <span className="badge badge-blue">Category</span>
                      </div>
                    );
                  }

                  return null;
                })}
            </div>
          </div>
        </div>
      )}
    </>
  );
}
