import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  PostDto,
  CreatePostRequest,
  UpdatePostRequest,
  CommentDto,
  CreateCommentRequest,
  PaginatedResult,
} from '../models/post.model';

@Injectable({ providedIn: 'root' })
export class PostService {
  private readonly API_URL = 'http://localhost:5167/api';

  constructor(private _http: HttpClient) {}

  list(title?: string, page = 0, size = 20): Observable<PaginatedResult<PostDto>> {
    let params = new HttpParams()
      .set('page', page)
      .set('size', size);

    if (title) {
      params = params.set('title', title);
    }

    return this._http.get<PaginatedResult<PostDto>>(`${this.API_URL}/posts`, { params });
  }

  getById(id: string): Observable<PostDto> {
    return this._http.get<PostDto>(`${this.API_URL}/posts/${id}`);
  }

  create(request: CreatePostRequest): Observable<PostDto> {
    return this._http.post<PostDto>(`${this.API_URL}/posts`, request);
  }

  update(id: string, request: UpdatePostRequest): Observable<PostDto> {
    return this._http.put<PostDto>(`${this.API_URL}/posts/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this._http.delete<void>(`${this.API_URL}/posts/${id}`);
  }

  listComments(postId: string): Observable<CommentDto[]> {
    return this._http.get<CommentDto[]>(`${this.API_URL}/posts/${postId}/comments`);
  }

  addComment(postId: string, request: CreateCommentRequest): Observable<CommentDto> {
    return this._http.post<CommentDto>(`${this.API_URL}/posts/${postId}/comments`, request);
  }
}
