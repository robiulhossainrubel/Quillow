"use client";

import React, { useState } from "react";
import { useQuery, useSubscription } from "@apollo/client/react";
import { GET_POSTS, GET_CATEGORIES, ON_POST_PUBLISHED } from "@/graphql/operations";
import { Navbar } from "@/components/Navbar";
import { StatsBanner } from "@/components/StatsBanner";
import { PostCard, PostItem } from "@/components/PostCard";
import { CreatePostModal } from "@/components/CreatePostModal";
import { AuthModal } from "@/components/AuthModal";
import { PostDetailModal } from "@/components/PostDetailModal";
import { Bell, Sparkles, Filter, RefreshCw } from "lucide-react";

export default function Home() {
  const [selectedCategory, setSelectedCategory] = useState<string>("ALL");
  const [selectedPostSlug, setSelectedPostSlug] = useState<string | null>(null);
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [liveToast, setLiveToast] = useState<{ title: string; author: string } | null>(null);

  // Queries
  const { data: postsData, loading: postsLoading, refetch } = useQuery<{ posts: PostItem[] }>(GET_POSTS);
  const { data: catData } = useQuery<{ categories: { id: string; name: string; slug: string }[] }>(GET_CATEGORIES);

  // 🔔 Real-time WebSocket Subscription for new published posts
  useSubscription<{ onPostPublished: { title: string; author?: { displayName: string } } }>(ON_POST_PUBLISHED, {
    onData: ({ data: subData }) => {
      const newPost = subData?.data?.onPostPublished;
      if (newPost) {
        setLiveToast({
          title: newPost.title,
          author: newPost.author?.displayName || "Author",
        });
        refetch();
        // Hide toast after 5s
        setTimeout(() => setLiveToast(null), 5000);
      }
    },
  });

  const posts: PostItem[] = postsData?.posts || [];
  const categories: { id: string; name: string; slug: string }[] = catData?.categories || [];

  const filteredPosts =
    selectedCategory === "ALL"
      ? posts
      : posts.filter((p) => p.category?.slug === selectedCategory);

  return (
    <div style={{ minHeight: "100vh", display: "flex", flexDirection: "column" }}>
      {/* Top Navbar */}
      <Navbar
        onOpenCreatePost={() => setIsCreateOpen(true)}
        onSelectPostBySlug={(slug) => setSelectedPostSlug(slug)}
      />

      {/* Floating Live WebSocket Notification Toast */}
      {liveToast && (
        <div
          style={{
            position: "fixed",
            bottom: "24px",
            right: "24px",
            zIndex: 1000,
            background: "rgba(19, 25, 38, 0.95)",
            backdropFilter: "blur(14px)",
            border: "1px solid var(--accent-primary)",
            boxShadow: "var(--shadow-glow)",
            borderRadius: "var(--radius-lg)",
            padding: "16px 20px",
            display: "flex",
            alignItems: "center",
            gap: "14px",
            animation: "slideUp 0.3s ease-out",
            maxWidth: "380px",
          }}
        >
          <div
            style={{
              width: "36px",
              height: "36px",
              borderRadius: "50%",
              background: "var(--accent-gradient)",
              display: "flex",
              alignItems: "center",
              justifyContent: "center",
            }}
          >
            <Bell size={18} color="#fff" />
          </div>
          <div>
            <div
              style={{
                fontSize: "0.75rem",
                color: "var(--emerald)",
                fontWeight: 700,
                display: "flex",
                alignItems: "center",
                gap: "5px",
              }}
            >
              <div className="live-dot" />
              <span>LIVE SUBSCRIPTION UPDATE</span>
            </div>
            <div style={{ fontSize: "0.9rem", fontWeight: 700, color: "#fff" }}>
              {liveToast.title}
            </div>
            <div style={{ fontSize: "0.75rem", color: "var(--text-faint)" }}>
              Published by {liveToast.author}
            </div>
          </div>
        </div>
      )}

      {/* Main Container */}
      <main style={{ flex: 1, maxWidth: "1280px", margin: "0 auto", padding: "0 24px 60px 24px", width: "100%" }}>
        {/* Hero & Stats */}
        <StatsBanner />

        {/* Filter Bar & Header */}
        <div
          style={{
            marginTop: "40px",
            marginBottom: "30px",
            display: "flex",
            alignItems: "center",
            justifyContent: "space-between",
            flexWrap: "wrap",
            gap: "16px",
          }}
        >
          {/* Category Filter Pills */}
          <div style={{ display: "flex", alignItems: "center", gap: "8px", flexWrap: "wrap" }}>
            <button
              onClick={() => setSelectedCategory("ALL")}
              style={{
                padding: "8px 16px",
                borderRadius: "var(--radius-full)",
                border: "1px solid",
                borderColor:
                  selectedCategory === "ALL"
                    ? "var(--accent-primary)"
                    : "var(--card-border)",
                background:
                  selectedCategory === "ALL"
                    ? "rgba(139, 92, 246, 0.2)"
                    : "rgba(255, 255, 255, 0.03)",
                color: selectedCategory === "ALL" ? "#fff" : "var(--text-muted)",
                fontSize: "0.85rem",
                fontWeight: 600,
                cursor: "pointer",
                transition: "all 0.2s ease",
              }}
            >
              All Topics ({posts.length})
            </button>

            {categories.map((cat) => (
              <button
                key={cat.id}
                onClick={() => setSelectedCategory(cat.slug)}
                style={{
                  padding: "8px 16px",
                  borderRadius: "var(--radius-full)",
                  border: "1px solid",
                  borderColor:
                    selectedCategory === cat.slug
                      ? "var(--accent-primary)"
                      : "var(--card-border)",
                  background:
                    selectedCategory === cat.slug
                      ? "rgba(139, 92, 246, 0.2)"
                      : "rgba(255, 255, 255, 0.03)",
                  color: selectedCategory === cat.slug ? "#fff" : "var(--text-muted)",
                  fontSize: "0.85rem",
                  fontWeight: 600,
                  cursor: "pointer",
                  transition: "all 0.2s ease",
                }}
              >
                {cat.name}
              </button>
            ))}
          </div>

          {/* Refetch / Refresh button */}
          <button
            className="btn-secondary"
            onClick={() => refetch()}
            style={{ padding: "8px 14px", fontSize: "0.825rem" }}
          >
            <RefreshCw size={14} />
            <span>Refresh</span>
          </button>
        </div>

        {/* Post Grid */}
        {postsLoading ? (
          <div
            style={{
              padding: "80px",
              textAlign: "center",
              color: "var(--text-faint)",
              fontSize: "1.1rem",
            }}
          >
            Fetching posts via Hot Chocolate DataLoader query...
          </div>
        ) : filteredPosts.length === 0 ? (
          <div
            className="glass-panel"
            style={{
              padding: "60px",
              textAlign: "center",
              borderRadius: "var(--radius-lg)",
            }}
          >
            <h3 style={{ fontSize: "1.2rem", marginBottom: "8px" }}>No posts in this category</h3>
            <p style={{ color: "var(--text-faint)", marginBottom: "20px" }}>
              Be the first author to publish in this topic!
            </p>
            <button className="btn-primary" onClick={() => setIsCreateOpen(true)}>
              Write a Post
            </button>
          </div>
        ) : (
          <div
            style={{
              display: "grid",
              gridTemplateColumns: "repeat(auto-fill, minmax(350px, 1fr))",
              gap: "24px",
            }}
          >
            {filteredPosts.map((post) => (
              <PostCard
                key={post.id}
                post={post}
                onSelect={(slug) => setSelectedPostSlug(slug)}
              />
            ))}
          </div>
        )}
      </main>

      {/* Footer */}
      <footer
        style={{
          borderTop: "1px solid var(--card-border)",
          padding: "30px 24px",
          textAlign: "center",
          color: "var(--text-faint)",
          fontSize: "0.875rem",
          background: "rgba(7, 9, 14, 0.7)",
        }}
      >
        <div style={{ maxWidth: "1280px", margin: "0 auto", display: "flex", justifyContent: "space-between", alignItems: "center", flexWrap: "wrap", gap: "12px" }}>
          <div>
            Built with <strong>Hot Chocolate GraphQL (.NET 10)</strong> & <strong>Next.js 15 (Apollo Client 3)</strong>
          </div>
          <div>
            Fullstack Clean Architecture Blog Platform
          </div>
        </div>
      </footer>

      {/* Modals */}
      <CreatePostModal isOpen={isCreateOpen} onClose={() => setIsCreateOpen(false)} />
      <AuthModal />
      <PostDetailModal slug={selectedPostSlug} onClose={() => setSelectedPostSlug(null)} />
    </div>
  );
}
