import { Component, OnInit } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-root',
  template: `
    <app-public-header *ngIf="!isAdminPanel"></app-public-header>
    <router-outlet></router-outlet>
  `,
  styles: []
})
export class AppComponent implements OnInit {
  title = 'nobat-ui';
  isAdminPanel = false;

  constructor(private router: Router) {}

  ngOnInit() {
    // بررسی مسیر فعلی
    this.checkRoute(this.router.url);
    
    // گوش دادن به تغییرات مسیر
    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe((event: any) => {
        this.checkRoute(event.url);
      });
  }

  checkRoute(url: string) {
    this.isAdminPanel = url.startsWith('/panel');
  }
}

