import { Component, OnInit, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { PostService } from '../../core/services/post.service';
import { PostDto, CommentDto } from '../../core/models/post.model';
import { PostEditComponent } from './post-edit.component';

@Component({
  selector: 'app-post-detail',
  standalone: true,
  imports: [DatePipe, RouterLink, ReactiveFormsModule, PostEditComponent],
  templateUrl: './post-detail.component.html',
})
export class PostDetailComponent implements OnInit {
  post = signal<PostDto | null>(null);
  comments = signal<CommentDto[]>([]);
  loading = signal(true);
  sendingComment = signal(false);

  // Controla o modal de edição e o popup de confirmar exclusão
  showEditModal = signal(false);
  showDeleteConfirm = signal(false);

  commentForm: FormGroup;

  constructor(
    private _route: ActivatedRoute,
    private _router: Router,
    private _postService: PostService,
    private _fb: FormBuilder
  ) {
    this.commentForm = this._fb.group({
      content: ['', [Validators.required, Validators.minLength(2)]],
    });
  }

  ngOnInit(): void {
    const id = this._route.snapshot.paramMap.get('id')!;

    this._postService.getById(id).subscribe({
      next: (post) => {
        this.post.set(post);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      },
    });

    this._postService.listComments(id).subscribe({
      next: (comments) => this.comments.set(comments),
    });
  }

  // ── Comentários ──

  onSubmitComment(): void {
    if (this.commentForm.invalid || !this.post()) return;

    this.sendingComment.set(true);

    this._postService.addComment(this.post()!.id, this.commentForm.value).subscribe({
      next: (newComment) => {
        this.comments.update((current) => [...current, newComment]);
        this.commentForm.reset();
        this.sendingComment.set(false);
      },
      error: () => {
        this.sendingComment.set(false);
      },
    });
  }

  // DELETE /api/posts/{postId}/comments/{commentId}
  // Remove o comentário da lista local sem recarregar a página
  deleteComment(commentId: string): void {
    this._postService.deleteComment(this.post()!.id, commentId).subscribe({
      next: () => {
        // .filter() cria uma lista nova SEM o comentário deletado
        // É como: setComments(prev => prev.filter(c => c.id !== commentId))
        this.comments.update((current) => current.filter((c) => c.id !== commentId));
      },
    });
  }

  // ── Editar post ──

  openEditModal(): void {
    this.showEditModal.set(true);
  }

  closeEditModal(): void {
    this.showEditModal.set(false);
  }

  onPostUpdated(): void {
    this.showEditModal.set(false);
    // Recarrega o post pra mostrar as alterações
    this._postService.getById(this.post()!.id).subscribe({
      next: (post) => this.post.set(post),
    });
  }

  // ── Deletar post ──

  confirmDelete(): void {
    this.showDeleteConfirm.set(true);
  }

  cancelDelete(): void {
    this.showDeleteConfirm.set(false);
  }

  // DELETE /api/posts/{id} → volta pra lista
  deletePost(): void {
    this._postService.delete(this.post()!.id).subscribe({
      next: () => this._router.navigate(['/posts']),
    });
  }
}
