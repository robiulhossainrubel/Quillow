import { gql } from "@apollo/client";

// ========== QUERIES ==========

export const GET_STATS = gql`
  query GetStats {
    stats {
      totalPosts
      publishedPosts
      draftPosts
      totalUsers
      totalComments
      totalLikes
      totalCategories
      totalTags
    }
  }
`;

export const GET_POSTS = gql`
  query GetPosts {
    posts {
      id
      title
      slug
      excerpt
      content
      coverImage
      viewCount
      isFeatured
      status
      createdAt
      publishedAt
      likeCount
      commentCount
      author {
        id
        displayName
        avatar
      }
      category {
        id
        name
        slug
      }
      tags {
        id
        name
        slug
      }
    }
  }
`;

export const GET_POST_BY_SLUG = gql`
  query GetPostBySlug($slug: String!) {
    postBySlug(slug: $slug) {
      id
      title
      slug
      content
      excerpt
      coverImage
      viewCount
      isFeatured
      status
      createdAt
      publishedAt
      likeCount
      commentCount
      author {
        id
        displayName
        email
        bio
        avatar
      }
      category {
        id
        name
        slug
      }
      tags {
        id
        name
      }
      comments {
        id
        content
        createdAt
        author {
          id
          displayName
          avatar
        }
        replies {
          id
          content
          createdAt
          author {
            id
            displayName
          }
        }
      }
    }
  }
`;

export const GET_CATEGORIES = gql`
  query GetCategories {
    categories {
      id
      name
      slug
      description
      postCount
    }
  }
`;

export const GET_ME = gql`
  query GetMe {
    me {
      id
      displayName
      email
      avatar
      bio
      roles
      postCount
      commentCount
    }
  }
`;

export const GLOBAL_SEARCH = gql`
  query GlobalSearch($term: String!) {
    search(term: $term) {
      ... on Post {
        id
        title
        slug
        excerpt
        author {
          displayName
        }
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
`;

// ========== MUTATIONS ==========

export const LOGIN_MUTATION = gql`
  mutation Login($input: LoginInput!) {
    login(input: $input) {
      token
      user {
        id
        displayName
        email
      }
      errors {
        message
        code
        field
      }
    }
  }
`;

export const REGISTER_MUTATION = gql`
  mutation Register($input: RegisterInput!) {
    register(input: $input) {
      token
      user {
        id
        displayName
        email
      }
      errors {
        message
        code
        field
      }
    }
  }
`;

export const CREATE_POST_MUTATION = gql`
  mutation CreatePost($input: CreatePostInput!) {
    createPost(input: $input) {
      post {
        id
        title
        slug
        content
        excerpt
        status
        publishedAt
        author {
          displayName
        }
        category {
          name
        }
      }
      errors {
        message
        code
        field
      }
    }
  }
`;

export const TOGGLE_LIKE_MUTATION = gql`
  mutation ToggleLike($postId: UUID!) {
    toggleLike(postId: $postId) {
      isLiked
      message
    }
  }
`;

export const ADD_COMMENT_MUTATION = gql`
  mutation AddComment($input: AddCommentInput!) {
    addComment(input: $input) {
      comment {
        id
        content
        createdAt
        author {
          id
          displayName
        }
      }
      errors {
        message
        code
      }
    }
  }
`;

// ========== PHASE 7: MEDIA OPERATIONS ==========

export const GET_MEDIA = gql`
  query GetMedia {
    media {
      id
      fileName
      url
      mimeType
      size
      createdAt
      uploadedBy {
        displayName
      }
    }
  }
`;

export const UPLOAD_MEDIA_MUTATION = gql`
  mutation UploadMedia($file: Upload!, $postId: UUID) {
    uploadMedia(file: $file, postId: $postId) {
      id
      fileName
      url
      mimeType
      size
    }
  }
`;

// ========== PHASE 5: OFFSET PAGINATION ==========

export const GET_POSTS_OFFSET = gql`
  query GetPostsOffset($skip: Int, $take: Int) {
    postsOffset(skip: $skip, take: $take) {
      items {
        id
        title
        slug
        excerpt
        createdAt
        author {
          displayName
        }
      }
      totalCount
      skip
      take
      hasNextPage
    }
  }
`;

// ========== SUBSCRIPTIONS ==========

export const ON_POST_PUBLISHED = gql`
  subscription OnPostPublished {
    onPostPublished {
      id
      title
      slug
      publishedAt
      author {
        displayName
      }
      category {
        name
      }
    }
  }
`;

export const ON_COMMENT_ADDED = gql`
  subscription OnCommentAdded($postId: UUID!) {
    onCommentAdded(postId: $postId) {
      id
      content
      createdAt
      author {
        id
        displayName
        avatar
      }
    }
  }
`;

export const ON_POST_LIKED = gql`
  subscription OnPostLiked($postId: UUID!) {
    onPostLiked(postId: $postId) {
      postId
      userId
      userDisplayName
      isLiked
      totalLikes
    }
  }
`;
