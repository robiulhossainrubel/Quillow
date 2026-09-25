"use client";

import React from "react";
import { useQuery } from "@apollo/client/react";
import { GET_STATS } from "@/graphql/operations";
import { BookOpen, Users, MessageSquare, Heart, Zap, Sparkles } from "lucide-react";

export function StatsBanner() {
  const { data, loading } = useQuery<{
    stats: {
      totalPosts: number;
      publishedPosts: number;
      draftPosts: number;
      totalUsers: number;
      totalComments: number;
      totalLikes: number;
      totalCategories: number;
      totalTags: number;
    };
  }>(GET_STATS, {
    pollInterval: 10000, // Poll stats periodically
  });

  const stats = data?.stats || {
    totalPosts: 4,
    publishedPosts: 3,
    totalUsers: 3,
    totalComments: 3,
    totalLikes: 3,
  };

  const statItems = [
    {
      label: "Published Posts",
      value: stats.publishedPosts,
      icon: BookOpen,
      color: "#a855f7",
      badge: "DataLoader Optimized",
    },
    {
      label: "Active Authors",
      value: stats.totalUsers,
      icon: Users,
      color: "#3b82f6",
      badge: "JWT + RBAC",
    },
    {
      label: "Comments & Replies",
      value: stats.totalComments,
      icon: MessageSquare,
      color: "#10b981",
      badge: "Live WebSocket",
    },
    {
      label: "Community Likes",
      value: stats.totalLikes,
      icon: Heart,
      color: "#ec4899",
      badge: "Real-time Push",
    },
  ];

  return (
    <section style={{ padding: "40px 0 20px 0" }}>
      {/* Hero Title */}
      <div style={{ textAlign: "center", marginBottom: "40px" }}>
        <div
          style={{
            display: "inline-flex",
            alignItems: "center",
            gap: "8px",
            padding: "6px 14px",
            borderRadius: "var(--radius-full)",
            background: "rgba(139, 92, 246, 0.12)",
            border: "1px solid rgba(139, 92, 246, 0.25)",
            fontSize: "0.825rem",
            fontWeight: 600,
            color: "#c084fc",
            marginBottom: "18px",
          }}
        >
          <Sparkles size={15} />
          <span>Fullstack GraphQL with .NET 10 + Hot Chocolate & Next.js 15</span>
        </div>
        <h1
          style={{
            fontSize: "clamp(2.2rem, 5vw, 3.6rem)",
            fontWeight: 800,
            lineHeight: 1.15,
            letterSpacing: "-0.03em",
            maxWidth: "800px",
            margin: "0 auto 16px auto",
          }}
        >
          Stories, Architecture & Ideas on <span className="gradient-text">Quillow</span>
        </h1>
        <p
          style={{
            fontSize: "1.1rem",
            color: "var(--text-muted)",
            maxWidth: "600px",
            margin: "0 auto",
          }}
        >
          An ultra-fast CMS powered by GraphQL Queries, Mutations, DataLoaders, and real-time WebSocket Subscriptions.
        </p>
      </div>

      {/* 4 Stat Cards */}
      <div
        style={{
          display: "grid",
          gridTemplateColumns: "repeat(auto-fit, minmax(240px, 1fr))",
          gap: "20px",
          maxWidth: "1100px",
          margin: "0 auto",
        }}
      >
        {statItems.map((item, i) => {
          const IconComponent = item.icon;
          return (
            <div
              key={i}
              className="glass-panel"
              style={{
                borderRadius: "var(--radius-lg)",
                padding: "24px",
                position: "relative",
                overflow: "hidden",
                transition: "all 0.3s ease",
              }}
              onMouseEnter={(e) => {
                e.currentTarget.style.transform = "translateY(-4px)";
                e.currentTarget.style.borderColor = item.color;
              }}
              onMouseLeave={(e) => {
                e.currentTarget.style.transform = "translateY(0)";
                e.currentTarget.style.borderColor = "var(--card-border)";
              }}
            >
              <div
                style={{
                  position: "absolute",
                  top: "-20px",
                  right: "-20px",
                  width: "90px",
                  height: "90px",
                  borderRadius: "50%",
                  background: item.color,
                  opacity: 0.1,
                  filter: "blur(20px)",
                }}
              />
              <div
                style={{
                  display: "flex",
                  alignItems: "center",
                  justifyContent: "space-between",
                  marginBottom: "12px",
                }}
              >
                <div
                  style={{
                    width: "42px",
                    height: "42px",
                    borderRadius: "var(--radius-md)",
                    background: `rgba(${item.color === "#a855f7" ? "168, 85, 247" : item.color === "#3b82f6" ? "59, 130, 246" : item.color === "#10b981" ? "16, 185, 129" : "236, 72, 153"}, 0.15)`,
                    display: "flex",
                    alignItems: "center",
                    justifyContent: "center",
                  }}
                >
                  <IconComponent size={20} color={item.color} />
                </div>
                <span
                  style={{
                    fontSize: "0.7rem",
                    fontWeight: 700,
                    letterSpacing: "0.04em",
                    color: "var(--text-faint)",
                    background: "rgba(255, 255, 255, 0.05)",
                    padding: "3px 8px",
                    borderRadius: "var(--radius-full)",
                  }}
                >
                  {item.badge}
                </span>
              </div>
              <div
                style={{
                  fontSize: "2.1rem",
                  fontWeight: 800,
                  color: "var(--text-main)",
                  lineHeight: 1,
                  marginBottom: "6px",
                }}
              >
                {loading ? "..." : item.value}
              </div>
              <div
                style={{
                  fontSize: "0.875rem",
                  color: "var(--text-muted)",
                  fontWeight: 500,
                }}
              >
                {item.label}
              </div>
            </div>
          );
        })}
      </div>
    </section>
  );
}
