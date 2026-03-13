using AicaseCli.Core;
using AicaseCli.Models;

namespace AicaseCli.Generators;

/// <summary>
/// Genera todo el código frontend Angular 17 para cada entidad del proyecto.
/// </summary>
public class FrontendGenerator
{
    private readonly TemplateEngine _engine;
    private readonly string _templatesPath;

    public FrontendGenerator(TemplateEngine engine, string templatesPath)
    {
        _engine = engine;
        _templatesPath = templatesPath;
    }

    public async Task GenerateAsync(ProjectDefinition project, string outputBasePath)
    {
        var frontendPath = Path.Combine(outputBasePath, "frontend");

        foreach (var entity in project.Entidades)
        {
            var model = BuildModel(project, entity);
            var entityKebab = ToKebabCase(entity.Nombre);

            await GenerateModelAsync(model, frontendPath, entityKebab);
            await GenerateServiceAsync(model, frontendPath, entityKebab);
            await GenerateListComponentAsync(model, frontendPath, entityKebab);
            await GenerateFormComponentAsync(model, frontendPath, entityKebab);
        }

        await GenerateRoutingModuleAsync(project, frontendPath);
        await GenerateAppModuleAsync(project, frontendPath);
        Console.WriteLine($"  → {project.Entidades.Count} entidades frontend generadas.");
    }

    private static object BuildModel(ProjectDefinition project, EntityDefinition entity) => new
    {
        Project = new
        {
            project.Nombre,
            project.Version,
            NombreFrontend = project.Configuracion?.NombreFrontend ?? ToKebabCase(project.Nombre)
        },
        Entity = entity,
        EntityKebab = ToKebabCase(entity.Nombre),
        EntityCamel = ToCamelCase(entity.Nombre)
    };

    private async Task GenerateModelAsync(object model, string frontendPath, string entityKebab)
    {
        var entity = ((dynamic)model).Entity as EntityDefinition;
        var outputPath = Path.Combine(frontendPath, "src", "app", "models", $"{entityKebab}.model.ts");

        var templatePath = Path.Combine(_templatesPath, "frontend", "model.scriban");
        if (File.Exists(templatePath))
            await _engine.RenderToFileAsync(templatePath, model, outputPath);
        else
            await _engine.RenderStringToFileAsync(GetEmbeddedModelTemplate(), model, outputPath);
    }

    private async Task GenerateServiceAsync(object model, string frontendPath, string entityKebab)
    {
        var outputPath = Path.Combine(frontendPath, "src", "app", "services", $"{entityKebab}.service.ts");

        var templatePath = Path.Combine(_templatesPath, "frontend", "service.scriban");
        if (File.Exists(templatePath))
            await _engine.RenderToFileAsync(templatePath, model, outputPath);
        else
            await _engine.RenderStringToFileAsync(GetEmbeddedServiceTemplate(), model, outputPath);
    }

    private async Task GenerateListComponentAsync(object model, string frontendPath, string entityKebab)
    {
        var componentDir = Path.Combine(frontendPath, "src", "app", "components", entityKebab, "list");

        var tsTmpl = Path.Combine(_templatesPath, "frontend", "list-component-ts.scriban");
        var htmlTmpl = Path.Combine(_templatesPath, "frontend", "list-component-html.scriban");

        var tsPath = Path.Combine(componentDir, $"{entityKebab}-list.component.ts");
        var htmlPath = Path.Combine(componentDir, $"{entityKebab}-list.component.html");

        if (File.Exists(tsTmpl))
            await _engine.RenderToFileAsync(tsTmpl, model, tsPath);
        else
            await _engine.RenderStringToFileAsync(GetEmbeddedListComponentTsTemplate(), model, tsPath);

        if (File.Exists(htmlTmpl))
            await _engine.RenderToFileAsync(htmlTmpl, model, htmlPath);
        else
            await _engine.RenderStringToFileAsync(GetEmbeddedListComponentHtmlTemplate(), model, htmlPath);
    }

    private async Task GenerateFormComponentAsync(object model, string frontendPath, string entityKebab)
    {
        var componentDir = Path.Combine(frontendPath, "src", "app", "components", entityKebab, "form");

        var tsTmpl = Path.Combine(_templatesPath, "frontend", "form-component-ts.scriban");
        var htmlTmpl = Path.Combine(_templatesPath, "frontend", "form-component-html.scriban");

        var tsPath = Path.Combine(componentDir, $"{entityKebab}-form.component.ts");
        var htmlPath = Path.Combine(componentDir, $"{entityKebab}-form.component.html");

        if (File.Exists(tsTmpl))
            await _engine.RenderToFileAsync(tsTmpl, model, tsPath);
        else
            await _engine.RenderStringToFileAsync(GetEmbeddedFormComponentTsTemplate(), model, tsPath);

        if (File.Exists(htmlTmpl))
            await _engine.RenderToFileAsync(htmlTmpl, model, htmlPath);
        else
            await _engine.RenderStringToFileAsync(GetEmbeddedFormComponentHtmlTemplate(), model, htmlPath);
    }

    private async Task GenerateRoutingModuleAsync(ProjectDefinition project, string frontendPath)
    {
        var outputPath = Path.Combine(frontendPath, "src", "app", "app-routing.module.ts");
        var model = new { Project = project, Entities = project.Entidades };

        var templatePath = Path.Combine(_templatesPath, "frontend", "routing-module.scriban");
        if (File.Exists(templatePath))
            await _engine.RenderToFileAsync(templatePath, model, outputPath);
        else
            await _engine.RenderStringToFileAsync(GetEmbeddedRoutingTemplate(), model, outputPath);
    }

    private static async Task GenerateAppModuleAsync(ProjectDefinition project, string frontendPath)
    {
        var outputPath = Path.Combine(frontendPath, "src", "app", "app.module.ts");
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

        var imports = new List<string>();
        var declarations = new List<string>();

        foreach (var entity in project.Entidades)
        {
            var kebab = ToKebabCase(entity.Nombre);
            imports.Add($"import {{ {entity.Nombre}ListComponent }} from './components/{kebab}/list/{kebab}-list.component';");
            imports.Add($"import {{ {entity.Nombre}FormComponent }} from './components/{kebab}/form/{kebab}-form.component';");
            declarations.Add($"    {entity.Nombre}ListComponent,");
            declarations.Add($"    {entity.Nombre}FormComponent,");
        }

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("import { NgModule } from '@angular/core';");
        sb.AppendLine("import { BrowserModule } from '@angular/platform-browser';");
        sb.AppendLine("import { BrowserAnimationsModule } from '@angular/platform-browser/animations';");
        sb.AppendLine("import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';");
        sb.AppendLine("import { ReactiveFormsModule } from '@angular/forms';");
        sb.AppendLine("import { MatTableModule } from '@angular/material/table';");
        sb.AppendLine("import { MatPaginatorModule } from '@angular/material/paginator';");
        sb.AppendLine("import { MatSortModule } from '@angular/material/sort';");
        sb.AppendLine("import { MatInputModule } from '@angular/material/input';");
        sb.AppendLine("import { MatButtonModule } from '@angular/material/button';");
        sb.AppendLine("import { MatSelectModule } from '@angular/material/select';");
        sb.AppendLine("import { MatCheckboxModule } from '@angular/material/checkbox';");
        sb.AppendLine("import { MatDatepickerModule } from '@angular/material/datepicker';");
        sb.AppendLine("import { MatNativeDateModule } from '@angular/material/core';");
        sb.AppendLine("import { MatSnackBarModule } from '@angular/material/snack-bar';");
        sb.AppendLine("import { MatDialogModule } from '@angular/material/dialog';");
        sb.AppendLine("import { MatIconModule } from '@angular/material/icon';");
        sb.AppendLine("import { AppRoutingModule } from './app-routing.module';");
        sb.AppendLine("import { AuthInterceptor } from './interceptors/auth.interceptor';");
        foreach (var imp in imports) sb.AppendLine(imp);
        sb.AppendLine();
        sb.AppendLine("@NgModule({");
        sb.AppendLine("  declarations: [");
        foreach (var decl in declarations) sb.AppendLine(decl);
        sb.AppendLine("  ],");
        sb.AppendLine("  imports: [");
        sb.AppendLine("    BrowserModule,");
        sb.AppendLine("    BrowserAnimationsModule,");
        sb.AppendLine("    HttpClientModule,");
        sb.AppendLine("    ReactiveFormsModule,");
        sb.AppendLine("    AppRoutingModule,");
        sb.AppendLine("    MatTableModule,");
        sb.AppendLine("    MatPaginatorModule,");
        sb.AppendLine("    MatSortModule,");
        sb.AppendLine("    MatInputModule,");
        sb.AppendLine("    MatButtonModule,");
        sb.AppendLine("    MatSelectModule,");
        sb.AppendLine("    MatCheckboxModule,");
        sb.AppendLine("    MatDatepickerModule,");
        sb.AppendLine("    MatNativeDateModule,");
        sb.AppendLine("    MatSnackBarModule,");
        sb.AppendLine("    MatDialogModule,");
        sb.AppendLine("    MatIconModule");
        sb.AppendLine("  ],");
        sb.AppendLine("  providers: [");
        sb.AppendLine("    { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true }");
        sb.AppendLine("  ],");
        sb.AppendLine("  bootstrap: []");
        sb.AppendLine("})");
        sb.AppendLine("export class AppModule { }");
        var content = sb.ToString();
        await File.WriteAllTextAsync(outputPath, content);
    }

    // ── Plantillas embebidas ─────────────────────────────────────────────────

    private static string GetEmbeddedModelTemplate() => """
// Generado por AICASE - No modificar manualmente

export interface {{ entity.nombre }} {
  {{- for campo in entity.campos }}
  {{ campo.nombre | to_camel_case }}: {{ campo.tipo | typescript_type }}{{ if !campo.requerido && !campo.es_pk }} | null{{ end }};
  {{- end }}
}

export interface {{ entity.nombre }}CreateDto {
  {{- for campo in entity.campos }}
  {{- if !campo.es_pk }}
  {{ campo.nombre | to_camel_case }}{{ if !campo.requerido }}?{{ end }}: {{ campo.tipo | typescript_type }}{{ if !campo.requerido }} | null{{ end }};
  {{- end }}
  {{- end }}
}

export interface {{ entity.nombre }}UpdateDto extends {{ entity.nombre }}CreateDto {
  id: number;
}
""";

    private static string GetEmbeddedServiceTemplate() => """
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService, PagedResult } from './api.service';
import { {{ entity.nombre }}, {{ entity.nombre }}CreateDto, {{ entity.nombre }}UpdateDto } from '../models/{{ entity_kebab }}.model';

@Injectable({ providedIn: 'root' })
export class {{ entity.nombre }}Service {
  private readonly endpoint = '{{ entity.api.ruta ?? "/api/" + entity_kebab + "s" }}';

  constructor(private api: ApiService) {}

  getAll(page = 1, pageSize = 10, filters?: Record<string, string>): Observable<PagedResult<{{ entity.nombre }}>> {
    return this.api.getAll<{{ entity.nombre }}>(this.endpoint, page, pageSize, filters);
  }

  getById(id: number): Observable<{{ entity.nombre }}> {
    return this.api.getById<{{ entity.nombre }}>(this.endpoint, id);
  }

  create(dto: {{ entity.nombre }}CreateDto): Observable<{{ entity.nombre }}> {
    return this.api.create<{{ entity.nombre }}>(this.endpoint, dto);
  }

  update(id: number, dto: {{ entity.nombre }}UpdateDto): Observable<{{ entity.nombre }}> {
    return this.api.update<{{ entity.nombre }}>(this.endpoint, id, dto);
  }

  delete(id: number): Observable<void> {
    return this.api.delete(this.endpoint, id);
  }
}
""";

    private static string GetEmbeddedListComponentTsTemplate() => """
import { Component, OnInit, ViewChild } from '@angular/core';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatSort, Sort } from '@angular/material/sort';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { {{ entity.nombre }} } from '../../../models/{{ entity_kebab }}.model';
import { {{ entity.nombre }}Service } from '../../../services/{{ entity_kebab }}.service';

@Component({
  selector: 'app-{{ entity_kebab }}-list',
  templateUrl: './{{ entity_kebab }}-list.component.html'
})
export class {{ entity.nombre }}ListComponent implements OnInit {
  items: {{ entity.nombre }}[] = [];
  totalCount = 0;
  page = 1;
  pageSize = {{ entity.pantallas.listado.paginacion.registros_por_pagina ?? 10 }};
  isLoading = false;
  searchTerm = '';

  displayedColumns: string[] = [{{ for col in entity.pantallas.listado.columnas }}'{{ col | to_camel_case }}', {{ end }}'acciones'];

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private service: {{ entity.nombre }}Service,
    private router: Router,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.isLoading = true;
    this.service.getAll(this.page, this.pageSize).subscribe({
      next: (result) => {
        this.items = result.items;
        this.totalCount = result.totalCount;
        this.isLoading = false;
      },
      error: (err) => {
        this.snackBar.open('Error al cargar los datos.', 'Cerrar', { duration: 3000 });
        this.isLoading = false;
      }
    });
  }

  onPageChange(event: PageEvent): void {
    this.page = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.loadData();
  }

  onEdit(id: number): void {
    this.router.navigate(['/{{ entity_kebab }}s', id, 'edit']);
  }

  onDelete(id: number): void {
    if (confirm('¿Está seguro de eliminar este registro?')) {
      this.service.delete(id).subscribe({
        next: () => {
          this.snackBar.open('Registro eliminado correctamente.', 'Cerrar', { duration: 3000 });
          this.loadData();
        },
        error: () => this.snackBar.open('Error al eliminar el registro.', 'Cerrar', { duration: 3000 })
      });
    }
  }

  onNew(): void {
    this.router.navigate(['/{{ entity_kebab }}s', 'new']);
  }
}
""";

    private static string GetEmbeddedListComponentHtmlTemplate() => """
<div class="container">
  <div class="header">
    <h2>{{ entity.pantallas.listado.titulo ?? entity.nombre + "s" }}</h2>
    <button mat-raised-button color="primary" (click)="onNew()">
      <mat-icon>add</mat-icon> Nuevo
    </button>
  </div>

  <mat-form-field appearance="outline" class="search-field">
    <mat-label>Buscar...</mat-label>
    <input matInput [(ngModel)]="searchTerm" (keyup.enter)="loadData()" />
    <mat-icon matSuffix>search</mat-icon>
  </mat-form-field>

  <div *ngIf="isLoading" class="loading">Cargando...</div>

  <table mat-table [dataSource]="items" matSort class="mat-elevation-z2 full-width">
    {{- for col in entity.pantallas.listado.columnas }}
    <ng-container matColumnDef="{{ col | to_camel_case }}">
      <th mat-header-cell *matHeaderCellDef mat-sort-header>{{ col }}</th>
      <td mat-cell *matCellDef="let row">{{ "{{" }} row.{{ col | to_camel_case }} {{ "}}" }}</td>
    </ng-container>
    {{- end }}

    <ng-container matColumnDef="acciones">
      <th mat-header-cell *matHeaderCellDef>Acciones</th>
      <td mat-cell *matCellDef="let row">
        <button mat-icon-button color="primary" (click)="onEdit(row.id)" title="Editar">
          <mat-icon>edit</mat-icon>
        </button>
        <button mat-icon-button color="warn" (click)="onDelete(row.id)" title="Eliminar">
          <mat-icon>delete</mat-icon>
        </button>
      </td>
    </ng-container>

    <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
    <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
    <tr class="mat-row" *matNoDataRow>
      <td class="mat-cell" [attr.colspan]="displayedColumns.length">No se encontraron registros.</td>
    </tr>
  </table>

  <mat-paginator
    [length]="totalCount"
    [pageSize]="pageSize"
    [pageSizeOptions]="{{ entity.pantallas.listado.paginacion.opciones_tamano ?? [10, 25, 50, 100] }}"
    (page)="onPageChange($event)">
  </mat-paginator>
</div>
""";

    private static string GetEmbeddedFormComponentTsTemplate() => """
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { {{ entity.nombre }}Service } from '../../../services/{{ entity_kebab }}.service';

@Component({
  selector: 'app-{{ entity_kebab }}-form',
  templateUrl: './{{ entity_kebab }}-form.component.html'
})
export class {{ entity.nombre }}FormComponent implements OnInit {
  form!: FormGroup;
  isEditMode = false;
  isLoading = false;
  recordId?: number;

  constructor(
    private fb: FormBuilder,
    private service: {{ entity.nombre }}Service,
    private route: ActivatedRoute,
    private router: Router,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.buildForm();
    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'new') {
      this.isEditMode = true;
      this.recordId = +id;
      this.loadRecord(this.recordId);
    }
  }

  buildForm(): void {
    this.form = this.fb.group({
      {{- for campo in entity.campos }}
      {{- if !campo.es_pk }}
      {{ campo.nombre | to_camel_case }}: [null{{ if campo.requerido }}, [Validators.required{{ if campo.largo }}, Validators.maxLength({{ campo.largo }}){{ end }}]{{ end }}],
      {{- end }}
      {{- end }}
    });
  }

  loadRecord(id: number): void {
    this.isLoading = true;
    this.service.getById(id).subscribe({
      next: (data) => { this.form.patchValue(data); this.isLoading = false; },
      error: () => { this.snackBar.open('Error al cargar el registro.', 'Cerrar', { duration: 3000 }); this.isLoading = false; }
    });
  }

  onSubmit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.isLoading = true;

    const obs = this.isEditMode && this.recordId
      ? this.service.update(this.recordId, { id: this.recordId, ...this.form.value })
      : this.service.create(this.form.value);

    obs.subscribe({
      next: () => {
        this.snackBar.open('Guardado correctamente.', 'Cerrar', { duration: 3000 });
        this.router.navigate(['/{{ entity_kebab }}s']);
      },
      error: () => {
        this.snackBar.open('Error al guardar.', 'Cerrar', { duration: 3000 });
        this.isLoading = false;
      }
    });
  }

  onCancel(): void {
    this.router.navigate(['/{{ entity_kebab }}s']);
  }
}
""";

    private static string GetEmbeddedFormComponentHtmlTemplate() => """
<div class="container">
  <h2>{{ entity.pantallas.formulario.titulo ?? (entity.nombre) }}</h2>

  <form [formGroup]="form" (ngSubmit)="onSubmit()">
    <div class="form-grid">
      {{- for campo in entity.campos }}
      {{- if !campo.es_pk && !campo.input.ocultar_en_formulario }}

      {{- if campo.input.tipo == "checkbox" }}
      <mat-checkbox formControlName="{{ campo.nombre | to_camel_case }}">
        {{ campo.input.label ?? campo.nombre }}
      </mat-checkbox>

      {{- else if campo.input.tipo == "datepicker" }}
      <mat-form-field appearance="outline">
        <mat-label>{{ campo.input.label ?? campo.nombre }}</mat-label>
        <input matInput [matDatepicker]="picker_{{ campo.nombre | to_camel_case }}"
               formControlName="{{ campo.nombre | to_camel_case }}"
               placeholder="{{ campo.input.placeholder ?? "" }}" />
        <mat-datepicker-toggle matIconSuffix [for]="picker_{{ campo.nombre | to_camel_case }}"></mat-datepicker-toggle>
        <mat-datepicker #picker_{{ campo.nombre | to_camel_case }}></mat-datepicker>
        <mat-error *ngIf="form.get('{{ campo.nombre | to_camel_case }}')?.hasError('required')">Campo requerido</mat-error>
      </mat-form-field>

      {{- else if campo.input.tipo == "dropdown" }}
      <mat-form-field appearance="outline">
        <mat-label>{{ campo.input.label ?? campo.nombre }}</mat-label>
        <mat-select formControlName="{{ campo.nombre | to_camel_case }}">
          <mat-option *ngFor="let item of {{ campo.input.fuente_datos | to_camel_case }}Options" [value]="item.id">
            {{ "{{" }} item.nombre {{ "}}" }}
          </mat-option>
        </mat-select>
        <mat-error *ngIf="form.get('{{ campo.nombre | to_camel_case }}')?.hasError('required')">Campo requerido</mat-error>
      </mat-form-field>

      {{- else if campo.input.tipo == "textarea" }}
      <mat-form-field appearance="outline" class="full-width">
        <mat-label>{{ campo.input.label ?? campo.nombre }}</mat-label>
        <textarea matInput formControlName="{{ campo.nombre | to_camel_case }}"
                  placeholder="{{ campo.input.placeholder ?? "" }}" rows="3"></textarea>
        <mat-error *ngIf="form.get('{{ campo.nombre | to_camel_case }}')?.hasError('required')">Campo requerido</mat-error>
      </mat-form-field>

      {{- else }}
      <mat-form-field appearance="outline">
        <mat-label>{{ campo.input.label ?? campo.nombre }}</mat-label>
        <input matInput formControlName="{{ campo.nombre | to_camel_case }}"
               type="{{ campo.input.tipo }}"
               placeholder="{{ campo.input.placeholder ?? "" }}" />
        <mat-error *ngIf="form.get('{{ campo.nombre | to_camel_case }}')?.hasError('required')">Campo requerido</mat-error>
        <mat-error *ngIf="form.get('{{ campo.nombre | to_camel_case }}')?.hasError('email')">Email inválido</mat-error>
        <mat-error *ngIf="form.get('{{ campo.nombre | to_camel_case }}')?.hasError('maxlength')">Excede longitud máxima</mat-error>
      </mat-form-field>
      {{- end }}

      {{- end }}
      {{- end }}
    </div>

    <div class="form-actions">
      <button mat-button type="button" (click)="onCancel()">Cancelar</button>
      <button mat-raised-button color="primary" type="submit" [disabled]="isLoading">
        {{ "{{" }} isEditMode ? 'Actualizar' : 'Guardar' {{ "}}" }}
      </button>
    </div>
  </form>
</div>
""";

    private static string GetEmbeddedRoutingTemplate() => """
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
{{- for entity in entities }}
import { {{ entity.nombre }}ListComponent } from './components/{{ entity.nombre | to_kebab_case }}/list/{{ entity.nombre | to_kebab_case }}-list.component';
import { {{ entity.nombre }}FormComponent } from './components/{{ entity.nombre | to_kebab_case }}/form/{{ entity.nombre | to_kebab_case }}-form.component';
{{- end }}

const routes: Routes = [
  { path: '', redirectTo: '{{ entities[0].nombre | to_kebab_case }}s', pathMatch: 'full' },
  {{- for entity in entities }}
  { path: '{{ entity.nombre | to_kebab_case }}s', component: {{ entity.nombre }}ListComponent },
  { path: '{{ entity.nombre | to_kebab_case }}s/new', component: {{ entity.nombre }}FormComponent },
  { path: '{{ entity.nombre | to_kebab_case }}s/:id/edit', component: {{ entity.nombre }}FormComponent },
  {{- end }}
  { path: '**', redirectTo: '{{ entities[0].nombre | to_kebab_case }}s' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
""";

    private static string ToCamelCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        return char.ToLowerInvariant(input[0]) + input[1..];
    }

    private static string ToKebabCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        return System.Text.RegularExpressions.Regex.Replace(
            input, "(?<=[a-z0-9])(?=[A-Z])", "-").ToLowerInvariant();
    }
}
