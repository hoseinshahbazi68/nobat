import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-loading',
  templateUrl: './loading.component.html',
  styleUrls: ['./loading.component.scss']
})
export class LoadingComponent {
  @Input() message: string = 'در حال بارگذاری...';
  @Input() size: 'small' | 'medium' | 'large' = 'medium';
  @Input() overlay: boolean = false;
}
