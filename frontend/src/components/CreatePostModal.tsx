"use client";

import React, { useState } from "react";
import { useMutation, useQuery } from "@apollo/client/react";
import { CREATE_POST_MUTATION, GET_CATEGORIES, GET_POSTS } from "@/graphql/operations";
import { X, Send, Sparkles, AlertCircle } from "lucide-react";

interface CreatePostModalProps {
  isOpen: boolean;
  onClose: () => void;
}

export function CreatePostModal({ isOpen, onClose }: CreatePostModalProps) {
  const [title, setTitle] = useState("");
  const [content, setContent] = useState("");
  const [excerpt, setExcerpt] = useState("");
  const [categoryId, setCategoryId] = useState("");
  const [publishNow, setPublishNow] = useState(true);
  const [errorMsg, setErrorMsg] = useState("");

  const { data: categoryData } = useQuery<{ categories: { id: string; name: string }[] }>(GET_CATEGORIES);
  const categories = categoryData?.categories || [];

  const [createPost, { loading }] = useMutation<{
    createPost: {
      post?: { id: string; title: string };
      errors?: { message: string }[];
    };
  }>(CREATE_POST_MUTATION, {
    refetchQueries: [{ query: GET_POSTS }],
    onCompleted: (data) => {
      const res = data?.createPost;
      if (res?.post) {
        onClose();
        setTitle("");
        setContent("");
        setExcerpt("");
        setCategoryId("");
      } else if (res?.errors?.length) {
        setErrorMsg(res.errors[0].message);
      }
    },
    onError: (err) => {
      setErrorMsg(err.message || "Failed to create post");
    },
  });

  if (!isOpen) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setErrorMsg("");

    if (!title.trim() || !content.trim()) {
      setErrorMsg("Title and content are required.");
      return;
    }

    const selectedCategory = categoryId || categories[0]?.id;
    if (!selectedCategory) {
      setErrorMsg("Please select a category.");
      return;
    }

    await createPost({
      variables: {
        input: {
          title: title.trim(),
          content: content.trim(),
          excerpt: excerpt.trim() || undefined,
          categoryId: selectedCategory,
          status: publishNow ? "PUBLISHED" : "DRAFT",
        },
      },
    });
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div
        className="modal-content"
        onClick={(e) => e.stopPropagation()}
        style={{ maxWidth: "680px" }}
      >
        {/* Modal Header */}
        <div
          style={{
            padding: "20px 24px",
            borderBottom: "1px solid var(--card-border)",
            display: "flex",
            alignItems: "center",
            justifyContent: "space-between",
          }}
        >
          <div style={{ display: "flex", alignItems: "center", gap: "10px" }}>
            <div
              style={{
                width: "36px",
                height: "36px",
                borderRadius: "var(--radius-md)",
                background: "var(--accent-gradient)",
                display: "flex",
                alignItems: "center",
                justifyContent: "center",
              }}
            >
              <Sparkles size={18} color="#fff" />
            </div>
            <div>
              <h2 style={{ fontSize: "1.2rem", fontWeight: 700 }}>Write New Post</h2>
              <p style={{ fontSize: "0.775rem", color: "var(--text-faint)" }}>
                Uses GraphQL Mutation with automatic JWT claims & WebSocket push
              </p>
            </div>
          </div>
          <button className="btn-icon" onClick={onClose}>
            <X size={18} />
          </button>
        </div>

        {/* Modal Body Form */}
        <form onSubmit={handleSubmit} style={{ padding: "24px" }}>
          {errorMsg && (
            <div
              style={{
                display: "flex",
                alignItems: "center",
                gap: "8px",
                padding: "12px 16px",
                borderRadius: "var(--radius-md)",
                background: "rgba(244, 63, 94, 0.12)",
                border: "1px solid rgba(244, 63, 94, 0.3)",
                color: "var(--rose)",
                fontSize: "0.875rem",
                marginBottom: "18px",
              }}
            >
              <AlertCircle size={18} />
              <span>{errorMsg}</span>
            </div>
          )}

          {/* Title */}
          <div className="form-group">
            <label className="form-label">Article Title *</label>
            <input
              type="text"
              className="form-input"
              placeholder="e.g. Understanding Hot Chocolate Subscriptions & WebSockets"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              required
            />
          </div>

          {/* Category */}
          <div className="form-group">
            <label className="form-label">Category *</label>
            <select
              className="form-input"
              value={categoryId || categories[0]?.id || ""}
              onChange={(e) => setCategoryId(e.target.value)}
              style={{ background: "#111624" }}
            >
              {categories.map((c: { id: string; name: string }) => (
                <option key={c.id} value={c.id}>
                  {c.name}
                </option>
              ))}
            </select>
          </div>

          {/* Excerpt */}
          <div className="form-group">
            <label className="form-label">Short Summary (Excerpt)</label>
            <input
              type="text"
              className="form-input"
              placeholder="A brief 1-2 sentence overview of your post"
              value={excerpt}
              onChange={(e) => setExcerpt(e.target.value)}
            />
          </div>

          {/* Content */}
          <div className="form-group">
            <label className="form-label">Article Content (Markdown supported) *</label>
            <textarea
              className="form-input"
              rows={8}
              placeholder="Write your article here..."
              value={content}
              onChange={(e) => setContent(e.target.value)}
              required
              style={{ resize: "vertical" }}
            />
          </div>

          {/* Publish Toggle */}
          <div
            style={{
              display: "flex",
              alignItems: "center",
              justifyContent: "space-between",
              padding: "12px 16px",
              borderRadius: "var(--radius-md)",
              background: "rgba(255, 255, 255, 0.03)",
              border: "1px solid var(--card-border)",
              marginBottom: "24px",
            }}
          >
            <div>
              <div style={{ fontSize: "0.9rem", fontWeight: 600 }}>Publish immediately</div>
              <div style={{ fontSize: "0.75rem", color: "var(--text-faint)" }}>
                Broadcasts live to all subscribers via WebSocket
              </div>
            </div>
            <input
              type="checkbox"
              checked={publishNow}
              onChange={(e) => setPublishNow(e.target.checked)}
              style={{ width: "18px", height: "18px", accentColor: "var(--accent-primary)" }}
            />
          </div>

          {/* Submit Actions */}
          <div style={{ display: "flex", justifyContent: "flex-end", gap: "12px" }}>
            <button type="button" className="btn-secondary" onClick={onClose}>
              Cancel
            </button>
            <button type="submit" className="btn-primary" disabled={loading}>
              <Send size={16} />
              <span>{loading ? "Publishing..." : "Publish Post"}</span>
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
