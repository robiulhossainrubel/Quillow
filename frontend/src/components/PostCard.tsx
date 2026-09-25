"use client";

import React, { useState } from "react";
import { useMutation } from "@apollo/client/react";
import { TOGGLE_LIKE_MUTATION } from "@/graphql/operations";
import { useAuth } from "@/context/AuthContext";
import { Heart, MessageSquare, Clock, Calendar, User, ArrowUpRight } from "lucide-react";

export interface PostItem {
  id: string;
  title: string;
  slug: string;
  excerpt: string;
  content: string;
  coverImage?: string;
  isFeatured?: boolean;
  status: string;
  createdAt: string;
  publishedAt?: string;
  likeCount: number;
  commentCount: number;
  author: {
    id: string;
    displayName: string;
    avatar?: string;
  };
  category: {
    id: string;
    name: string;
    slug: string;
  };
  tags?: {
    id: string;
    name: string;
    slug: string;
  }[];
}

interface PostCardProps {
  post: PostItem;
  onSelect: (slug: string) => void;
}

export function PostCard({ post, onSelect }: PostCardProps) {
  const { user, openLoginModal } = useAuth();
  const [likes, setLikes] = useState(post.likeCount || 0);
  const [isLiked, setIsLiked] = useState(false);

  const [toggleLikeMutation] = useMutation(TOGGLE_LIKE_MUTATION);

  const handleLike = async (e: React.MouseEvent) => {
    e.stopPropagation();
    if (!user) {
      openLoginModal();
      return;
    }

    // Optimistic UI update
    const nextLiked = !isLiked;
    setIsLiked(nextLiked);
    setLikes((prev) => (nextLiked ? prev + 1 : Math.max(0, prev - 1)));

    try {
      await toggleLikeMutation({
        variables: { postId: post.id },
      });
    } catch {
      // Revert if mutation fails
      setIsLiked(!nextLiked);
      setLikes((prev) => (nextLiked ? Math.max(0, prev - 1) : prev + 1));
    }
  };

  const readingTime = Math.max(1, Math.ceil((post.content?.split(" ").length || 100) / 200));

  const formattedDate = new Date(post.publishedAt || post.createdAt).toLocaleDateString("en-US", {
    month: "short",
    day: "numeric",
    year: "numeric",
  });

  return (
    <article
      className="glass-panel"
      onClick={() => onSelect(post.slug)}
      style={{
        borderRadius: "var(--radius-lg)",
        padding: "24px",
        cursor: "pointer",
        display: "flex",
        flexDirection: "column",
        justifyContent: "space-between",
        transition: "all 0.3s cubic-bezier(0.16, 1, 0.3, 1)",
        position: "relative",
      }}
      onMouseEnter={(e) => {
        e.currentTarget.style.transform = "translateY(-5px)";
        e.currentTarget.style.borderColor = "var(--card-hover-border)";
        e.currentTarget.style.boxShadow = "var(--shadow-glow)";
      }}
      onMouseLeave={(e) => {
        e.currentTarget.style.transform = "translateY(0)";
        e.currentTarget.style.borderColor = "var(--card-border)";
        e.currentTarget.style.boxShadow = "var(--shadow-card)";
      }}
    >
      <div>
        {/* Category & Date Header */}
        <div
          style={{
            display: "flex",
            alignItems: "center",
            justifyContent: "space-between",
            marginBottom: "14px",
          }}
        >
          <span className="badge badge-purple">{post.category?.name || "Tech"}</span>
          <div
            style={{
              display: "flex",
              alignItems: "center",
              gap: "5px",
              color: "var(--text-faint)",
              fontSize: "0.8rem",
            }}
          >
            <Clock size={13} />
            <span>{readingTime} min read</span>
          </div>
        </div>

        {/* Title */}
        <h3
          style={{
            fontSize: "1.28rem",
            fontWeight: 700,
            lineHeight: 1.35,
            color: "var(--text-main)",
            marginBottom: "10px",
            letterSpacing: "-0.015em",
          }}
        >
          {post.title}
        </h3>

        {/* Excerpt */}
        <p
          style={{
            color: "var(--text-muted)",
            fontSize: "0.925rem",
            lineHeight: 1.55,
            marginBottom: "20px",
            display: "-webkit-box",
            WebkitLineClamp: 3,
            WebkitBoxOrient: "vertical",
            overflow: "hidden",
          }}
        >
          {post.excerpt || post.content}
        </p>

        {/* Tags */}
        {post.tags && post.tags.length > 0 && (
          <div
            style={{
              display: "flex",
              flexWrap: "wrap",
              gap: "6px",
              marginBottom: "20px",
            }}
          >
            {post.tags.map((t) => (
              <span
                key={t.id}
                style={{
                  fontSize: "0.725rem",
                  color: "var(--text-faint)",
                  background: "rgba(255, 255, 255, 0.04)",
                  padding: "2px 8px",
                  borderRadius: "var(--radius-sm)",
                }}
              >
                #{t.name}
              </span>
            ))}
          </div>
        )}
      </div>

      {/* Footer: Author & Social Actions */}
      <div
        style={{
          borderTop: "1px solid rgba(255, 255, 255, 0.06)",
          paddingTop: "16px",
          display: "flex",
          alignItems: "center",
          justifyContent: "space-between",
        }}
      >
        {/* Author */}
        <div style={{ display: "flex", alignItems: "center", gap: "10px" }}>
          <div
            style={{
              width: "34px",
              height: "34px",
              borderRadius: "50%",
              background: "var(--accent-gradient)",
              display: "flex",
              alignItems: "center",
              justifyContent: "center",
              color: "#fff",
              fontWeight: 700,
              fontSize: "0.85rem",
            }}
          >
            {post.author?.displayName?.[0] || "U"}
          </div>
          <div>
            <div
              style={{
                fontSize: "0.875rem",
                fontWeight: 600,
                color: "var(--text-main)",
              }}
            >
              {post.author?.displayName}
            </div>
            <div
              style={{
                fontSize: "0.725rem",
                color: "var(--text-faint)",
              }}
            >
              {formattedDate}
            </div>
          </div>
        </div>

        {/* Actions */}
        <div style={{ display: "flex", alignItems: "center", gap: "12px" }}>
          {/* Like Button */}
          <button
            onClick={handleLike}
            style={{
              background: isLiked
                ? "rgba(244, 63, 94, 0.15)"
                : "rgba(255, 255, 255, 0.05)",
              border: isLiked
                ? "1px solid rgba(244, 63, 94, 0.4)"
                : "1px solid var(--card-border)",
              color: isLiked ? "var(--rose)" : "var(--text-muted)",
              padding: "6px 10px",
              borderRadius: "var(--radius-md)",
              display: "flex",
              alignItems: "center",
              gap: "6px",
              fontSize: "0.825rem",
              fontWeight: 600,
              cursor: "pointer",
              transition: "all 0.2s ease",
            }}
          >
            <Heart
              size={15}
              fill={isLiked ? "var(--rose)" : "none"}
              color={isLiked ? "var(--rose)" : "currentColor"}
            />
            <span>{likes}</span>
          </button>

          {/* Comment Count */}
          <div
            style={{
              display: "flex",
              alignItems: "center",
              gap: "5px",
              color: "var(--text-faint)",
              fontSize: "0.825rem",
            }}
          >
            <MessageSquare size={15} />
            <span>{post.commentCount || 0}</span>
          </div>
        </div>
      </div>
    </article>
  );
}
