export interface PostDto {
  id: string;
  title: string;
  content: string;
  authorId: string;
  authorName: string;
  isPublished: boolean;
  publishedAt: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface CreatePostRequest {
  title: string;
  content: string;
}

export interface UpdatePostRequest {
  title: string;
  content: string;
  isPublished: boolean;
}

export interface CommentDto {
  id: string;
  content: string;
  authorId: string;
  authorName: string;
  createdAt: string;
}

export interface CreateCommentRequest {
  content: string;
}
