"use client";

import React, { useState, useEffect } from "react";
import { useQuery, useMutation, useSubscription } from "@apollo/client/react";
import {
  GET_POST_BY_SLUG,
  ADD_COMMENT_MUTATION,
  TOGGLE_LIKE_MUTATION,
  ON_COMMENT_ADDED,
  ON_POST_LIKED,
} from "@/graphql/operations";
import { useAuth } from "@/context/AuthContext";
import {
  X,
  Heart,
  MessageSquare,
  Clock,
  Calendar,
  Send,
  User as UserIcon,
  Tag,
  Radio,
} from "lucide-react";

interface PostDetailModalProps {
  slug: string | null;
  onClose: () => void;
}

interface CommentItem {
  id: string;
  content: string;
  createdAt: string;
  author: {
    id: string;
    displayName: string;
    avatar?: string;
  };
  replies?: {
    id: string;
    content: string;
    createdAt: string;
    author: {
      displayName: string;
    };
  }[];
}

export function PostDetailModal({ slug, onClose }: PostDetailModalProps) {
  const { user, openLoginModal } = useAuth();
  const [commentText, setCommentText] = useState("");
  const [liveComments, setLiveComments] = useState<CommentItem[]>([]);
  const [likesCount, setLikesCount] = useState(0);
  const [isLiked, setIsLiked] = useState(false);

  const { data, loading } = useQuery<{
    postBySlug: {
      id: string;
      title: string;
      slug: string;
      content: string;
      excerpt?: string;
      coverImage?: string;
      likeCount: number;
      commentCount: number;
      author: { id: string; displayName: string; email?: string; bio?: string };
      category: { id: string; name: string; slug: string };
      tags?: { id: string; name: string }[];
      comments: CommentItem[];
    };
  }>(GET_POST_BY_SLUG, {
    variables: { slug },
    skip: !slug,
  });

  const post = data?.postBySlug;

  // Initialize comments and likes once post loads
  useEffect(() => {
    if (post) {
      setLiveComments(post.comments || []);
      setLikesCount(post.likeCount || 0);
    }
  }, [post]);

  // 🔔 Real-time GraphQL WebSocket Subscription for new Comments
  useSubscription<{ onCommentAdded: CommentItem }>(ON_COMMENT_ADDED, {
    variables: { postId: post?.id },
    skip: !post?.id,
    onData: ({ data: subData }) => {
      const newComment = subData?.data?.onCommentAdded;
      if (newComment) {
        setLiveComments((prev) => {
          // Avoid duplicate comments
          if (prev.some((c) => c.id === newComment.id)) return prev;
          return [newComment, ...prev];
        });
      }
    },
  });

  // 🔔 Real-time GraphQL WebSocket Subscription for Likes
  useSubscription<{ onPostLiked: { totalLikes: number; isLiked: boolean } }>(ON_POST_LIKED, {
    variables: { postId: post?.id },
    skip: !post?.id,
    onData: ({ data: subData }) => {
      const likeInfo = subData?.data?.onPostLiked;
      if (likeInfo) {
        setLikesCount(likeInfo.totalLikes);
      }
    },
  });

  // Mutation: Add Comment
  const [addCommentMutation, { loading: addingComment }] = useMutation(ADD_COMMENT_MUTATION);

  // Mutation: Toggle Like
  const [toggleLikeMutation] = useMutation(TOGGLE_LIKE_MUTATION);

  if (!slug) return null;

  const handleAddComment = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!user) {
      openLoginModal();
      return;
    }
    if (!commentText.trim() || !post?.id) return;

    try {
      await addCommentMutation({
        variables: {
          input: {
            postId: post.id,
            content: commentText.trim(),
          },
        },
      });
      setCommentText("");
    } catch (err) {
      console.error("Comment failed:", err);
    }
  };

  const handleToggleLike = async () => {
    if (!user) {
      openLoginModal();
      return;
    }
    if (!post?.id) return;

    const nextLiked = !isLiked;
    setIsLiked(nextLiked);
    setLikesCount((prev) => (nextLiked ? prev + 1 : Math.max(0, prev - 1)));

    try {
      await toggleLikeMutation({
        variables: { postId: post.id },
      });
    } catch {
      setIsLiked(!nextLiked);
    }
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div
        className="modal-content"
        onClick={(e) => e.stopPropagation()}
        style={{ maxWidth: "780px", maxHeight: "92vh" }}
      >
        {/* Modal Header */}
        <div
          style={{
            padding: "18px 24px",
            borderBottom: "1px solid var(--card-border)",
            display: "flex",
            alignItems: "center",
            justifyContent: "space-between",
            position: "sticky",
            top: 0,
            background: "rgba(13, 17, 26, 0.95)",
            backdropFilter: "blur(12px)",
            zIndex: 10,
          }}
        >
          <div style={{ display: "flex", alignItems: "center", gap: "10px" }}>
            <span className="badge badge-purple">{post?.category?.name || "Article"}</span>
            <div
              style={{
                display: "flex",
                alignItems: "center",
                gap: "6px",
                fontSize: "0.75rem",
                color: "var(--emerald)",
                fontWeight: 600,
              }}
            >
              <div className="live-dot" />
              <span>WebSocket Subscription Active</span>
            </div>
          </div>
          <button className="btn-icon" onClick={onClose}>
            <X size={18} />
          </button>
        </div>

        {/* Content Body */}
        {loading || !post ? (
          <div style={{ padding: "60px", textAlign: "center", color: "var(--text-faint)" }}>
            Loading article via DataLoader query...
          </div>
        ) : (
          <div style={{ padding: "28px" }}>
            {/* Title */}
            <h1
              style={{
                fontSize: "clamp(1.5rem, 3.5vw, 2.1rem)",
                fontWeight: 800,
                lineHeight: 1.25,
                color: "var(--text-main)",
                marginBottom: "16px",
              }}
            >
              {post.title}
            </h1>

            {/* Author info & Actions */}
            <div
              style={{
                display: "flex",
                alignItems: "center",
                justifyContent: "space-between",
                paddingBottom: "20px",
                borderBottom: "1px solid rgba(255, 255, 255, 0.08)",
                marginBottom: "28px",
                flexWrap: "wrap",
                gap: "16px",
              }}
            >
              <div style={{ display: "flex", alignItems: "center", gap: "12px" }}>
                <div
                  style={{
                    width: "44px",
                    height: "44px",
                    borderRadius: "50%",
                    background: "var(--accent-gradient)",
                    display: "flex",
                    alignItems: "center",
                    justifyContent: "center",
                    color: "#fff",
                    fontWeight: 700,
                    fontSize: "1.1rem",
                  }}
                >
                  {post.author?.displayName?.[0] || "U"}
                </div>
                <div>
                  <div style={{ fontWeight: 700, fontSize: "1rem" }}>
                    {post.author?.displayName}
                  </div>
                  <div style={{ fontSize: "0.8rem", color: "var(--text-faint)" }}>
                    {post.author?.bio || post.author?.email}
                  </div>
                </div>
              </div>

              {/* Like action */}
              <button
                onClick={handleToggleLike}
                style={{
                  display: "flex",
                  alignItems: "center",
                  gap: "8px",
                  background: isLiked
                    ? "rgba(244, 63, 94, 0.15)"
                    : "rgba(255, 255, 255, 0.05)",
                  border: isLiked
                    ? "1px solid rgba(244, 63, 94, 0.4)"
                    : "1px solid var(--card-border)",
                  color: isLiked ? "var(--rose)" : "var(--text-main)",
                  padding: "8px 16px",
                  borderRadius: "var(--radius-full)",
                  cursor: "pointer",
                  fontWeight: 600,
                  fontSize: "0.9rem",
                  transition: "all 0.2s ease",
                }}
              >
                <Heart
                  size={18}
                  fill={isLiked ? "var(--rose)" : "none"}
                  color={isLiked ? "var(--rose)" : "currentColor"}
                />
                <span>{likesCount} Likes</span>
              </button>
            </div>

            {/* Article Content */}
            <div
              style={{
                fontSize: "1.05rem",
                lineHeight: 1.8,
                color: "#e2e8f0",
                whiteSpace: "pre-wrap",
                marginBottom: "36px",
              }}
            >
              {post.content}
            </div>

            {/* Tags */}
            {post.tags && post.tags.length > 0 && (
              <div
                style={{
                  display: "flex",
                  flexWrap: "wrap",
                  gap: "8px",
                  paddingBottom: "24px",
                  borderBottom: "1px solid rgba(255, 255, 255, 0.08)",
                  marginBottom: "32px",
                }}
              >
                {post.tags.map((t: { id: string; name: string }) => (
                  <span key={t.id} className="badge badge-purple">
                    <Tag size={12} />
                    <span>{t.name}</span>
                  </span>
                ))}
              </div>
            )}

            {/* Comments Section */}
            <div>
              <div
                style={{
                  display: "flex",
                  alignItems: "center",
                  justifyContent: "space-between",
                  marginBottom: "20px",
                }}
              >
                <h3
                  style={{
                    fontSize: "1.2rem",
                    fontWeight: 700,
                    display: "flex",
                    alignItems: "center",
                    gap: "8px",
                  }}
                >
                  <MessageSquare size={18} color="var(--accent-primary)" />
                  <span>Comments ({liveComments.length})</span>
                </h3>
                <span
                  style={{
                    fontSize: "0.75rem",
                    color: "var(--emerald)",
                    display: "flex",
                    alignItems: "center",
                    gap: "5px",
                  }}
                >
                  <Radio size={12} />
                  <span>Real-time Subscriptions Active</span>
                </span>
              </div>

              {/* Add Comment Box */}
              <form onSubmit={handleAddComment} style={{ marginBottom: "28px" }}>
                <div style={{ position: "relative" }}>
                  <textarea
                    rows={3}
                    className="form-input"
                    placeholder={
                      user
                        ? "Write a comment... (instant WebSocket broadcast to all readers)"
                        : "Sign in to leave a comment"
                    }
                    value={commentText}
                    onChange={(e) => setCommentText(e.target.value)}
                    disabled={!user || addingComment}
                    style={{ paddingRight: "60px" }}
                  />
                  <button
                    type="submit"
                    className="btn-primary"
                    disabled={!user || !commentText.trim() || addingComment}
                    style={{
                      position: "absolute",
                      right: "10px",
                      bottom: "12px",
                      padding: "8px 14px",
                    }}
                  >
                    <Send size={15} />
                  </button>
                </div>
              </form>

              {/* Comment List */}
              <div style={{ display: "flex", flexDirection: "column", gap: "16px" }}>
                {liveComments.length === 0 ? (
                  <div
                    style={{
                      padding: "20px",
                      textAlign: "center",
                      color: "var(--text-faint)",
                      background: "rgba(255, 255, 255, 0.02)",
                      borderRadius: "var(--radius-md)",
                    }}
                  >
                    No comments yet. Be the first to share your thoughts!
                  </div>
                ) : (
                  liveComments.map((comment) => (
                    <div
                      key={comment.id}
                      style={{
                        padding: "16px 20px",
                        background: "rgba(255, 255, 255, 0.03)",
                        border: "1px solid var(--card-border)",
                        borderRadius: "var(--radius-md)",
                      }}
                    >
                      <div
                        style={{
                          display: "flex",
                          alignItems: "center",
                          justifyContent: "space-between",
                          marginBottom: "8px",
                        }}
                      >
                        <div style={{ display: "flex", alignItems: "center", gap: "8px" }}>
                          <div
                            style={{
                              width: "28px",
                              height: "28px",
                              borderRadius: "50%",
                              background: "rgba(139, 92, 246, 0.3)",
                              display: "flex",
                              alignItems: "center",
                              justifyContent: "center",
                              fontSize: "0.75rem",
                              fontWeight: 700,
                            }}
                          >
                            {comment.author?.displayName?.[0] || "U"}
                          </div>
                          <span style={{ fontWeight: 600, fontSize: "0.9rem" }}>
                            {comment.author?.displayName}
                          </span>
                        </div>
                        <span style={{ fontSize: "0.75rem", color: "var(--text-faint)" }}>
                          {new Date(comment.createdAt).toLocaleTimeString([], {
                            hour: "2-digit",
                            minute: "2-digit",
                          })}
                        </span>
                      </div>
                      <p style={{ fontSize: "0.925rem", color: "var(--text-muted)" }}>
                        {comment.content}
                      </p>
                    </div>
                  ))
                )}
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
