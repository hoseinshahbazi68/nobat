import { ApplicationRef, ComponentFactoryResolver, ComponentRef, Injectable, Injector } from '@angular/core';
import { Observable, Subject } from 'rxjs';
import { ConfirmDialogComponent, ConfirmDialogData } from '../components/confirm-dialog/confirm-dialog.component';

export interface DialogConfig {
  width?: string;
  maxWidth?: string;
  data?: any;
  position?: {
    top?: string;
    right?: string;
    left?: string;
    bottom?: string;
  };
  transparentOverlay?: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class DialogService {
  private currentDialog?: ComponentRef<any>;
  private resultSubject = new Subject<any>();

  constructor(
    private componentFactoryResolver: ComponentFactoryResolver,
    private injector: Injector,
    private appRef: ApplicationRef
  ) { }

  open(component: any, config: DialogConfig = {}): Observable<any> {
    // Create overlay
    const overlay = document.createElement('div');
    overlay.className = 'dialog-overlay';
    const overlayBackground = config.transparentOverlay ? 'transparent' : 'rgba(0, 0, 0, 0.5)';
    const overlayBackdrop = config.transparentOverlay ? 'none' : 'blur(4px)';
    overlay.style.cssText = `
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: ${overlayBackground};
      backdrop-filter: ${overlayBackdrop};
      z-index: 10000;
      display: flex;
      align-items: ${config.position ? 'flex-start' : 'center'};
      justify-content: ${config.position ? 'flex-start' : 'center'};
      animation: fadeIn 0.2s ease;
    `;

    // Create dialog wrapper
    const dialogWrapper = document.createElement('div');
    dialogWrapper.className = 'dialog-wrapper';
    const width = config.width || '600px';
    const maxWidth = config.maxWidth || '90vw';

    let positionStyle = '';
    if (config.position) {
      positionStyle = `
        position: absolute;
        ${config.position.top ? `top: ${config.position.top};` : ''}
        ${config.position.right ? `right: ${config.position.right};` : ''}
        ${config.position.left ? `left: ${config.position.left};` : ''}
        ${config.position.bottom ? `bottom: ${config.position.bottom};` : ''}
      `;
    }

    const borderRadius = config.width === 'auto' ? '12px' : '24px';
    const padding = config.width === 'auto' ? '0' : 'auto';
    dialogWrapper.style.cssText = `
      background: ${config.width === 'auto' ? 'transparent' : 'white'};
      border-radius: ${borderRadius};
      box-shadow: ${config.width === 'auto' ? 'none' : '0 25px 50px -12px rgba(0, 0, 0, 0.25)'};
      max-width: ${maxWidth};
      width: ${width};
      max-height: 90vh;
      overflow: ${config.width === 'auto' ? 'visible' : 'hidden'};
      display: flex;
      flex-direction: column;
      animation: slideUp 0.3s cubic-bezier(0.4, 0, 0.2, 1);
      position: relative;
      padding: ${padding};
      ${positionStyle}
    `;

    // Create component
    const factory = this.componentFactoryResolver.resolveComponentFactory(component);
    const componentRef = factory.create(this.injector);

    // Set data if provided
    const instance = componentRef.instance as any;
    if (config.data !== undefined) {
      // Set data property if it exists
      if ('data' in instance) {
        instance.data = config.data;
      }
      // Also set user property if it exists (for UserClinicsDialogComponent)
      if ('user' in instance) {
        instance.user = config.data;
      }
      // Also set clinic property if it exists (for ClinicUsersDialogComponent)
      if ('clinic' in instance) {
        instance.clinic = config.data;
      }
      // Also set specialty property if it exists (for SpecialtyMedicalConditionsDialogComponent)
      if ('specialty' in instance) {
        instance.specialty = config.data;
      }
      // Also set doctor property if it exists (for AdminChangeDoctorPasswordDialogComponent)
      if ('doctor' in instance) {
        instance.doctor = config.data?.doctor || config.data;
      }
      // Also set userId property if it exists (for AdminChangeDoctorPasswordDialogComponent)
      if ('userId' in instance) {
        // Try to get userId from config.data.userId (when passed as {doctor, userId}) or from config.data.userId (when passed as doctor object)
        const userId = config.data?.userId ?? config.data?.doctor?.userId;
        if (userId !== undefined && userId !== null) {
          instance.userId = userId;
        }
      }
    }

    // Handle close
    instance.dialogRef = {
      close: (result?: any) => {
        this.close(result);
      }
    };

    // Attach to view
    this.appRef.attachView(componentRef.hostView);
    dialogWrapper.appendChild(componentRef.location.nativeElement);
    overlay.appendChild(dialogWrapper);
    document.body.appendChild(overlay);

    this.currentDialog = componentRef;

    // Close on overlay click
    overlay.addEventListener('click', (e) => {
      if (e.target === overlay) {
        this.close();
      }
    });

    // Add animations if not already added
    if (!document.getElementById('dialog-animations')) {
      const style = document.createElement('style');
      style.id = 'dialog-animations';
      style.textContent = `
        @keyframes fadeIn {
          from { opacity: 0; }
          to { opacity: 1; }
        }
        @keyframes slideUp {
          from {
            opacity: 0;
            transform: translateY(20px) scale(0.95);
          }
          to {
            opacity: 1;
            transform: translateY(0) scale(1);
          }
        }
      `;
      document.head.appendChild(style);
    }

    return this.resultSubject.asObservable();
  }

  close(result?: any) {
    if (this.currentDialog) {
      const overlay = document.querySelector('.dialog-overlay');
      if (overlay) {
        overlay.remove();
      }
      this.appRef.detachView(this.currentDialog.hostView);
      this.currentDialog.destroy();
      this.currentDialog = undefined;
      this.resultSubject.next(result);
      this.resultSubject.complete();
      this.resultSubject = new Subject<any>();
    }
  }

  confirm(data: ConfirmDialogData): Observable<boolean> {
    return this.open(ConfirmDialogComponent, {
      width: '450px',
      maxWidth: '90vw',
      data: data
    });
  }
}
