"use client";

import React, { useState } from "react";
import { useAuth } from "@/context/AuthContext";
import { X, Lock, Mail, User, Shield, AlertCircle, Sparkles } from "lucide-react";

export function AuthModal() {
  const {
    isAuthModalOpen,
    setIsAuthModalOpen,
    authModalMode,
    setAuthModalMode,
    login,
    register,
  } = useAuth();

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [userName, setUserName] = useState("");
  const [displayName, setDisplayName] = useState("");
  const [errorMsg, setErrorMsg] = useState("");
  const [submitting, setSubmitting] = useState(false);

  if (!isAuthModalOpen) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setErrorMsg("");
    setSubmitting(true);

    if (authModalMode === "login") {
      const res = await login(email, password);
      if (!res.success) {
        setErrorMsg(res.error || "Login failed");
      }
    } else {
      const res = await register(userName, email, password, displayName);
      if (!res.success) {
        setErrorMsg(res.error || "Registration failed");
      }
    }
    setSubmitting(false);
  };

  const handleQuickLogin = (demoEmail: string, demoPass: string) => {
    setEmail(demoEmail);
    setPassword(demoPass);
    login(demoEmail, demoPass);
  };

  return (
    <div className="modal-overlay" onClick={() => setIsAuthModalOpen(false)}>
      <div
        className="modal-content"
        onClick={(e) => e.stopPropagation()}
        style={{ maxWidth: "460px" }}
      >
        {/* Header */}
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
              <Lock size={18} color="#fff" />
            </div>
            <div>
              <h2 style={{ fontSize: "1.2rem", fontWeight: 700 }}>
                {authModalMode === "login" ? "Welcome Back" : "Join Quillow"}
              </h2>
              <p style={{ fontSize: "0.775rem", color: "var(--text-faint)" }}>
                JWT Authentication & Role-Based Authorization
              </p>
            </div>
          </div>
          <button className="btn-icon" onClick={() => setIsAuthModalOpen(false)}>
            <X size={18} />
          </button>
        </div>

        {/* Form Body */}
        <div style={{ padding: "24px" }}>
          {/* Quick Demo Login Pills */}
          {authModalMode === "login" && (
            <div
              style={{
                background: "rgba(139, 92, 246, 0.08)",
                border: "1px solid rgba(139, 92, 246, 0.2)",
                borderRadius: "var(--radius-md)",
                padding: "12px 14px",
                marginBottom: "20px",
              }}
            >
              <div
                style={{
                  fontSize: "0.75rem",
                  color: "#c084fc",
                  fontWeight: 700,
                  marginBottom: "8px",
                  display: "flex",
                  alignItems: "center",
                  gap: "6px",
                }}
              >
                <Sparkles size={14} />
                <span>1-CLICK DEMO ACCOUNTS</span>
              </div>
              <div style={{ display: "flex", gap: "8px" }}>
                <button
                  type="button"
                  onClick={() => handleQuickLogin("admin@quillow.com", "Admin@123")}
                  style={{
                    flex: 1,
                    background: "rgba(255, 255, 255, 0.06)",
                    border: "1px solid var(--card-border)",
                    color: "var(--text-main)",
                    padding: "6px 10px",
                    borderRadius: "var(--radius-sm)",
                    fontSize: "0.775rem",
                    fontWeight: 600,
                    cursor: "pointer",
                  }}
                >
                  👑 Admin User
                </button>
                <button
                  type="button"
                  onClick={() => handleQuickLogin("robiul@quillow.com", "Password@123")}
                  style={{
                    flex: 1,
                    background: "rgba(255, 255, 255, 0.06)",
                    border: "1px solid var(--card-border)",
                    color: "var(--text-main)",
                    padding: "6px 10px",
                    borderRadius: "var(--radius-sm)",
                    fontSize: "0.775rem",
                    fontWeight: 600,
                    cursor: "pointer",
                  }}
                >
                  ✍️ Robiul (Author)
                </button>
              </div>
            </div>
          )}

          {errorMsg && (
            <div
              style={{
                display: "flex",
                alignItems: "center",
                gap: "8px",
                padding: "10px 14px",
                borderRadius: "var(--radius-md)",
                background: "rgba(244, 63, 94, 0.12)",
                border: "1px solid rgba(244, 63, 94, 0.3)",
                color: "var(--rose)",
                fontSize: "0.85rem",
                marginBottom: "18px",
              }}
            >
              <AlertCircle size={16} />
              <span>{errorMsg}</span>
            </div>
          )}

          <form onSubmit={handleSubmit}>
            {authModalMode === "register" && (
              <>
                <div className="form-group">
                  <label className="form-label">Username *</label>
                  <input
                    type="text"
                    className="form-input"
                    placeholder="john_doe"
                    value={userName}
                    onChange={(e) => setUserName(e.target.value)}
                    required
                  />
                </div>
                <div className="form-group">
                  <label className="form-label">Display Name *</label>
                  <input
                    type="text"
                    className="form-input"
                    placeholder="John Doe"
                    value={displayName}
                    onChange={(e) => setDisplayName(e.target.value)}
                    required
                  />
                </div>
              </>
            )}

            <div className="form-group">
              <label className="form-label">Email Address *</label>
              <input
                type="email"
                className="form-input"
                placeholder="name@example.com"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
              />
            </div>

            <div className="form-group">
              <label className="form-label">Password *</label>
              <input
                type="password"
                className="form-input"
                placeholder="••••••••"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
              />
            </div>

            <button
              type="submit"
              className="btn-primary"
              disabled={submitting}
              style={{ width: "100%", padding: "12px", marginTop: "10px" }}
            >
              {submitting
                ? "Processing..."
                : authModalMode === "login"
                ? "Sign In"
                : "Create Account"}
            </button>
          </form>

          {/* Toggle Login / Register */}
          <div
            style={{
              textAlign: "center",
              marginTop: "20px",
              fontSize: "0.875rem",
              color: "var(--text-faint)",
            }}
          >
            {authModalMode === "login" ? (
              <>
                Don&apos;t have an account?{" "}
                <span
                  style={{
                    color: "var(--accent-primary)",
                    fontWeight: 600,
                    cursor: "pointer",
                  }}
                  onClick={() => {
                    setErrorMsg("");
                    setAuthModalMode("register");
                  }}
                >
                  Sign Up
                </span>
              </>
            ) : (
              <>
                Already have an account?{" "}
                <span
                  style={{
                    color: "var(--accent-primary)",
                    fontWeight: 600,
                    cursor: "pointer",
                  }}
                  onClick={() => {
                    setErrorMsg("");
                    setAuthModalMode("login");
                  }}
                >
                  Sign In
                </span>
              </>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
