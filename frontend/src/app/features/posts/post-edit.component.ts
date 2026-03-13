import { Component, Input, Output, EventEmitter, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { PostService } from '../../core/services/post.service';
import { PostDto } from '../../core/models/post.model';

@Component({
  selector: 'app-post-edit',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './post-edit.component.html',
})
export class PostEditComponent implements OnInit {
  // @Input = o pai MANDA o post pra editar (colchetes no template)
  // No React seria: function PostEdit({ post }) { ... }
  @Input() post!: PostDto;

  // @Output = o filho AVISA o pai quando terminou
  @Output() closed = new EventEmitter<void>();
  @Output() updated = new EventEmitter<void>();

  form: FormGroup;
  loading = signal(false);
  errorMessage = signal('');

  constructor(
    private _fb: FormBuilder,
    private _postService: PostService
  ) {
    // Cria o form vazio primeiro — preenche no ngOnInit
    this.form = this._fb.group({
      title: ['', [Validators.required, Validators.minLength(3)]],
      content: ['', [Validators.required, Validators.minLength(10)]],
    });
  }

  // Quando o componente aparece, preenche o form com os dados do post
  // O @Input já foi recebido quando o ngOnInit roda
  ngOnInit(): void {
    this.form.patchValue({
      title: this.post.title,
      content: this.post.content,
    });
  }

  onSubmit(): void {
    if (this.form.invalid) return;

    this.loading.set(true);
    this.errorMessage.set('');

    // PUT /api/posts/{id} com { title, content, isPublished }
    this._postService.update(this.post.id, {
      ...this.form.value,
      isPublished: this.post.isPublished,
    }).subscribe({
      next: () => this.updated.emit(),
      error: (err) => {
        this.loading.set(false);
        this.errorMessage.set(err.error?.message || 'Erro ao atualizar o post.');
      },
    });
  }

  close(): void {
    this.closed.emit();
  }
}
