import { Component, Output, EventEmitter, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { PostService } from '../../core/services/post.service';

@Component({
  selector: 'app-post-create',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './post-create.component.html',
})
export class PostCreateComponent {
  // ──────────────────────────────────────────
  // @Output = o jeito do Angular de um FILHO falar com o PAI
  // ──────────────────────────────────────────
  // No React seria: props.onClose() e props.onCreated()
  // No Angular: this.closed.emit() e this.created.emit()
  //
  // O PAI (post-list) escuta assim no template:
  //   <app-post-create (closed)="fecharModal()" (created)="recarregarLista()" />
  @Output() closed = new EventEmitter<void>();
  @Output() created = new EventEmitter<void>();

  // FormGroup = formulário inteligente com validação embutida
  form: FormGroup;

  // Signals pro Angular 21 zoneless detectar mudanças dentro do subscribe
  loading = signal(false);
  errorMessage = signal('');

  constructor(
    private _fb: FormBuilder,
    private _postService: PostService
  ) {
    this.form = this._fb.group({
      title: ['', [Validators.required, Validators.minLength(3)]],
      content: ['', [Validators.required, Validators.minLength(10)]],
    });
  }

  onSubmit(): void {
    if (this.form.invalid) return;

    this.loading.set(true);
    this.errorMessage.set('');

    // POST http://localhost:5167/api/posts com { title, content }
    this._postService.create(this.form.value).subscribe({
      next: () => {
        // Deu certo → avisa o PAI que criou (pra ele recarregar a lista)
        this.created.emit();
      },
      error: (err) => {
        this.loading.set(false);
        this.errorMessage.set(err.error?.message || 'Erro ao criar o post. Tente novamente.');
      },
    });
  }

  // Fecha o modal (clicou no X ou no fundo escuro)
  close(): void {
    this.closed.emit();
  }
}
