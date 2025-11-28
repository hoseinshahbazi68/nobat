import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subject } from 'rxjs';

@Component({
  selector: 'app-layout',
  templateUrl: './layout.component.html',
  styleUrls: ['./layout.component.scss']
})
export class LayoutComponent implements OnInit, OnDestroy {
  isMobile = false;
  sidebarOpen = false;
  private destroy$ = new Subject<void>();

  ngOnInit() {
    this.checkBreakpoint();
    window.addEventListener('resize', () => this.checkBreakpoint());
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
    window.removeEventListener('resize', () => this.checkBreakpoint());
  }

  private checkBreakpoint() {
    this.isMobile = window.innerWidth < 960;
    if (!this.isMobile) {
      this.sidebarOpen = true;
    } else {
      this.sidebarOpen = false;
    }
  }

  toggleSidenav() {
    this.sidebarOpen = !this.sidebarOpen;
  }

  onSidenavClose() {
    if (this.isMobile) {
      this.sidebarOpen = false;
    }
  }
}

