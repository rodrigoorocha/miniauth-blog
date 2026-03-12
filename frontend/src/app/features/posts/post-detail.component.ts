import { Component, OnInit, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { PostService } from '../../core/services/post.service';
import { PostDto, CommentDto } from '../../core/models/post.model';

@Component({
  selector: 'app-post-detail',
  standalone: true,
  imports: [DatePipe, RouterLink, ReactiveFormsModule],
  templateUrl: './post-detail.component.html',
})
export class PostDetailComponent implements OnInit {
  // ──────────────────────────────────────────
  // PASSO 1: Estado do componente (tudo com Signal pro Angular 21 zoneless)
  // ──────────────────────────────────────────
  // No React seria:
  // const [post, setPost] = useState(null);
  // const [comments, setComments] = useState([]);
  // const [loading, setLoading] = useState(true);
  post = signal<PostDto | null>(null);
  comments = signal<CommentDto[]>([]);
  loading = signal(true);
  sendingComment = signal(false);

  // Formulário do comentário — só tem 1 campo: content
  commentForm: FormGroup;

  // ──────────────────────────────────────────
  // PASSO 2: Injeção de dependência
  // ──────────────────────────────────────────
  // ActivatedRoute = dá acesso aos parâmetros da URL
  // No React seria: const { id } = useParams()
  // No Angular: this._route.snapshot.paramMap.get('id')
  constructor(
    private _route: ActivatedRoute,
    private _postService: PostService,
    private _fb: FormBuilder
  ) {
    this.commentForm = this._fb.group({
      content: ['', [Validators.required, Validators.minLength(2)]],
    });
  }

  // ──────────────────────────────────────────
  // PASSO 3: Quando o componente aparece, busca o post e os comentários
  // ──────────────────────────────────────────
  // No React seria:
  // useEffect(() => {
  //   const id = params.id;
  //   fetch(`/api/posts/${id}`).then(res => res.json()).then(data => setPost(data));
  //   fetch(`/api/posts/${id}/comments`).then(res => res.json()).then(data => setComments(data));
  // }, [params.id]);
  ngOnInit(): void {
    // Pega o "id" da URL: /posts/abc-123 → id = "abc-123"
    const id = this._route.snapshot.paramMap.get('id')!;

    // Busca o post: GET /api/posts/abc-123
    this._postService.getById(id).subscribe({
      next: (post) => {
        this.post.set(post);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      },
    });

    // Busca os comentários: GET /api/posts/abc-123/comments
    this._postService.listComments(id).subscribe({
      next: (comments) => this.comments.set(comments),
    });
  }

  // ──────────────────────────────────────────
  // PASSO 4: Enviar um novo comentário
  // ──────────────────────────────────────────
  // POST /api/posts/{id}/comments com body: { content: "..." }
  // Quando dá certo, adiciona o comentário novo na lista SEM recarregar tudo
  onSubmitComment(): void {
    if (this.commentForm.invalid || !this.post()) return;

    this.sendingComment.set(true);

    this._postService.addComment(this.post()!.id, this.commentForm.value).subscribe({
      next: (newComment) => {
        // Adiciona o comentário novo no final da lista
        // .update() é como o setState(prev => [...prev, novo]) do React
        this.comments.update((current) => [...current, newComment]);
        this.commentForm.reset();
        this.sendingComment.set(false);
      },
      error: () => {
        this.sendingComment.set(false);
      },
    });
  }
}
