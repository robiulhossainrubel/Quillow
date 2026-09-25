# 🪶 Quillow — Enterprise Fullstack GraphQL Blog & CMS Platform

> **Status:** 100% COMPLETE (All 10 Phases Implemented & Verified)  
> **Backend:** .NET 10 Web API + Hot Chocolate 16 + Entity Framework Core 10 + PostgreSQL  
> **Frontend:** Next.js 15 (App Router, TypeScript) + Apollo Client 4 + Glassmorphism Dark Theme  
> **Real-time:** GraphQL-WS WebSockets Subscriptions + In-Memory Pub/Sub  

---

## 🏗️ Architecture & Clean Design

```
📁 GraphQL/
├── 📁 src/
│   ├── 📁 Quillow.Core/             # Pure Domain Models (Post, User, Comment, Category, Tag, Like, Media)
│   ├── 📁 Quillow.Infrastructure/   # EF Core DbContext, PostgreSQL Provider, Migrations, Seeders
│   ├── 📁 Quillow.Services/         # Business Logic, Password Hashing (BCrypt), JWT Token Service
│   └── 📁 Quillow.API/              # Hot Chocolate 16 GraphQL Server, Banana Cake Pop, DataLoaders
├── 📁 frontend/                     # Next.js 15 App Router, Apollo Client 4, Vanilla CSS Theme
├── 📁 tests/
│   └── 📁 Quillow.Tests/            # xUnit Test Suite (Schema, Auth, Validation Tests)
├── docker-compose.yml               # Multi-container orchestration (DB + API + Web)
└── README.md
```

---

## 📚 GraphQL Concepts Implemented (100% Roadmap)

| # | Concept | Implementation Location |
|---|---|---|
| 1 | **Schema Definition (SDL)** | Hot Chocolate code-first schema with Banana Cake Pop explorer |
| 2 | **Object Types** | `PostType`, `UserType`, `CategoryType`, `CommentType`, `TagType`, `LikeType`, `MediaType` |
| 3 | **Queries (Read)** | `posts`, `postById`, `postBySlug`, `featuredPosts`, `categories`, `tags`, `me`, `stats` |
| 4 | **Mutations (CUD)** | `register`, `login`, `createPost`, `updatePost`, `deletePost`, `addComment`, `toggleLike`, `uploadMedia` |
| 5 | **Subscriptions (Real-time)** | `onPostPublished`, `onCommentAdded(postId)`, `onPostLiked(postId)` via WebSocket |
| 6 | **Input Types** | `CreatePostInput`, `UpdatePostInput`, `RegisterInput`, `LoginInput`, `AddCommentInput` |
| 7 | **Enum Types** | `PostStatus` (`DRAFT`, `PUBLISHED`, `ARCHIVED`) |
| 8 | **Interface Types** | `ITimestamped` (`CreatedAt`, `UpdatedAt`) implemented by `Post` and `Comment` |
| 9 | **Union Types** | `SearchResult = Post \| User \| Category` for global polymorphic search |
| 10 | **DataLoaders (N+1 Solution)** | `AuthorByIdDataLoader`, `CategoryByIdDataLoader`, `CommentsByPostIdDataLoader`, `LikeCountByPostIdDataLoader`, `TagsByPostIdDataLoader` |
| 11 | **Cursor Pagination** | Relay-style cursor connections (`first`, `after`, `edges`, `node`, `pageInfo`) |
| 12 | **Offset Pagination** | Traditional `skip` / `take` pagination (`postsOffset`) |
| 13 | **Filtering & Sorting** | `[UseFiltering]`, `[UseSorting]`, `[UseProjections]` |
| 14 | **Authentication** | JWT Bearer Authentication (`Microsoft.AspNetCore.Authentication.JwtBearer`) |
| 15 | **Authorization** | `[Authorize]`, `[Authorize(Roles = ["Admin"])]`, `ClaimsPrincipal` context resolution |
| 16 | **File Upload (Phase 7)** | GraphQL Multipart Request Spec, `UploadType`, `IFile` scalar, static serving at `/uploads` |
| 17 | **Security & Hardening (Phase 8)** | `AddMaxExecutionDepthRule(15)`, `AddCostAnalyzer()` complexity limiting, request timeouts |
| 18 | **Optimistic UI (Phase 9)** | Instant client-side like counters in Next.js before server ack |
| 19 | **Automated Tests (Phase 10)** | xUnit suite verifying Schema generation, BCrypt hashing, JWT tokens, and input validation |
| 20 | **Containerization (Phase 10)** | Multi-stage Dockerfiles + `docker-compose.yml` for PostgreSQL, API, and Frontend |

---

## ⚡ Quick Start

### Option A: Run with Docker Compose (Recommended)

To run the entire fullstack platform (PostgreSQL, .NET 10 API, and Next.js Frontend) in one command:

```bash
docker compose up -d
```

- **Frontend Application:** [http://localhost:3000](http://localhost:3000)
- **GraphQL Banana Cake Pop Playground:** [http://localhost:5049/graphql](http://localhost:5049/graphql)
- **PostgreSQL Database:** `localhost:5432` (`QuilllowDb`)

---

### Option B: Run Locally with .NET SDK & Node.js

#### 1. Backend (.NET 10 Web API)
```bash
# Ensure PostgreSQL is running on port 5432
dotnet run --project src/Quillow.API/Quillow.API.csproj
```
The API automatically applies EF Core migrations and seeds initial users, posts, categories, and tags!

#### 2. Frontend (Next.js 15)
```bash
cd frontend
npm install
npm run dev
```
Open [http://localhost:3000](http://localhost:3000) (or `http://localhost:3001` if port 3000 is occupied).

#### 3. Run Automated Tests
```bash
dotnet test
```
**Test Results:** 5/5 Passed (100% Green).

---

## 🔑 Demo Credentials

| Role | Email | Password | Permissions |
|---|---|---|---|
| **Admin** | `admin@quillow.com` | `Admin@123` | Full CRUD, Create Categories, Upload Media |
| **Author** | `author@quillow.com` | `Password@123` | Create & Edit Own Posts, Comments, Likes |
| **Reader** | `reader@quillow.com` | `Password@123` | Read Posts, Add Comments, Toggle Likes |

The Frontend Navbar includes a **"1-Click Demo Login"** button for instant switching between roles!

---

## 🧪 Example GraphQL Operations

### 1. Global Search (Union Type)
```graphql
query GlobalSearch {
  search(term: "GraphQL") {
    __typename
    ... on Post {
      id
      title
      slug
      excerpt
    }
    ... on User {
      id
      displayName
      email
    }
    ... on Category {
      id
      name
      slug
    }
  }
}
```

### 2. Live Comment Subscription (WebSocket)
```graphql
subscription OnCommentAdded($postId: UUID!) {
  onCommentAdded(postId: $postId) {
    id
    content
    createdAt
    author {
      displayName
      avatar
    }
  }
}
```

### 3. File Upload Mutation (Multipart Spec)
```graphql
mutation UploadMedia($file: Upload!, $postId: UUID) {
  uploadMedia(file: $file, postId: $postId) {
    id
    fileName
    url
    mimeType
    size
  }
}
```

---

## 🏆 Project Plan Roadmap Status

All 10 Phases detailed in `project_plan.md` have been fulfilled to 100%:
- [x] **Phase 1:** Project Foundation & Clean Architecture
- [x] **Phase 2:** Basic Queries, Object Types & Fragments
- [x] **Phase 3:** Mutations, Input Types & Error Payloads
- [x] **Phase 4:** JWT Authentication & Role-Based Authorization
- [x] **Phase 5:** Advanced Queries (DataLoaders, Pagination, Unions, Interfaces)
- [x] **Phase 6:** Real-time Subscriptions with WebSockets
- [x] **Phase 7:** File Uploads & Media Storage
- [x] **Phase 8:** Query Depth Limiting, Cost Analysis & Performance Hardening
- [x] **Phase 9:** Next.js 15 + Apollo Client 4 Fullstack UI
- [x] **Phase 10:** Automated Unit/Integration Tests & Docker Containerization
