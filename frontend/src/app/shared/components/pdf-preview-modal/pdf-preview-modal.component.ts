import { Component, Input, Output, EventEmitter, inject, signal, OnChanges, SimpleChanges, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';

@Component({
  selector: 'app-pdf-preview-modal',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './pdf-preview-modal.component.html',
  styleUrl: './pdf-preview-modal.component.scss'
})
export class PdfPreviewModalComponent implements OnChanges, OnDestroy {
  private sanitizer = inject(DomSanitizer);

  @Input() isOpen = false;
  @Input() title = 'Belge Önizleme';
  @Input() pdfBlob: Blob | null = null;
  @Input() fileName = 'Belge.pdf';
  @Input() isLoading = false;

  @Output() closed = new EventEmitter<void>();

  safePdfUrl = signal<SafeResourceUrl | null>(null);
  private rawObjectUrl: string | null = null;

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['pdfBlob'] && this.pdfBlob) {
      this.cleanupUrl();
      this.rawObjectUrl = URL.createObjectURL(this.pdfBlob);
      this.safePdfUrl.set(this.sanitizer.bypassSecurityTrustResourceUrl(this.rawObjectUrl));
    }
  }

  ngOnDestroy(): void {
    this.cleanupUrl();
  }

  close(): void {
    this.cleanupUrl();
    this.closed.emit();
  }

  downloadPdf(): void {
    if (!this.pdfBlob) return;
    const blobUrl = URL.createObjectURL(this.pdfBlob);
    const link = document.createElement('a');
    link.href = blobUrl;
    link.download = this.fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    setTimeout(() => URL.revokeObjectURL(blobUrl), 1000);
  }

  printPdf(): void {
    if (!this.rawObjectUrl) return;
    const iframe = document.createElement('iframe');
    iframe.style.display = 'none';
    iframe.src = this.rawObjectUrl;
    document.body.appendChild(iframe);
    iframe.onload = () => {
      iframe.contentWindow?.print();
      setTimeout(() => document.body.removeChild(iframe), 2000);
    };
  }

  private cleanupUrl(): void {
    if (this.rawObjectUrl) {
      URL.revokeObjectURL(this.rawObjectUrl);
      this.rawObjectUrl = null;
      this.safePdfUrl.set(null);
    }
  }
}
