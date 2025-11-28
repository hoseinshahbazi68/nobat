import { Component, OnInit } from '@angular/core';
import { SettingsService, AppSettings } from '../../services/settings.service';
import { DialogService } from '../../services/dialog.service';

@Component({
  selector: 'app-settings',
  templateUrl: './settings.component.html',
  styleUrls: ['./settings.component.scss']
})
export class SettingsComponent implements OnInit {
  isOpen = false;
  settings: AppSettings;
  availableFonts: string[] = [];
  availableColors: { name: string; value: string }[] = [];

  constructor(
    private settingsService: SettingsService,
    private dialogService: DialogService
  ) {
    this.settings = this.settingsService.getSettings();
  }

  ngOnInit() {
    this.availableFonts = this.settingsService.getAvailableFonts();
    this.availableColors = this.settingsService.getAvailableColors();

    this.settingsService.settings$.subscribe(settings => {
      this.settings = settings;
    });
  }

  toggleSettings() {
    this.isOpen = !this.isOpen;
  }

  closeSettings() {
    this.isOpen = false;
  }

  updateFontFamily(event: Event) {
    const target = event.target as HTMLSelectElement;
    this.settingsService.updateSettings({ fontFamily: target.value });
  }

  updatePrimaryColor(color: string) {
    this.settingsService.updateSettings({ primaryColor: color });
  }

  updateSecondaryColor(color: string) {
    this.settingsService.updateSettings({ secondaryColor: color });
  }

  updateFontSize(size: string) {
    this.settingsService.updateSettings({ fontSize: size });
  }

  resetSettings() {
    this.dialogService.confirm({
      title: 'بازگردانی تنظیمات',
      message: 'آیا می‌خواهید تنظیمات را به حالت پیش‌فرض بازگردانید؟',
      confirmText: 'بازگردانی',
      cancelText: 'انصراف',
      type: 'warning'
    }).subscribe(result => {
      if (result) {
        this.settingsService.resetSettings();
      }
    });
  }
}
