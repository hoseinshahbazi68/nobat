import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { SnackbarService } from '../../services/snackbar.service';
import { PersianDateService } from '../../services/persian-date.service';

@Component({
  selector: 'app-generate-schedule',
  templateUrl: './generate-schedule.component.html',
  styleUrls: ['./generate-schedule.component.scss']
})
export class GenerateScheduleComponent implements OnInit {
  generateForm: FormGroup;
  doctors: any[] = [];
  shifts: any[] = [];
  isGenerating = false;

  constructor(
    private fb: FormBuilder,
    private snackbarService: SnackbarService,
    private persianDateService: PersianDateService
  ) {
    this.generateForm = this.fb.group({
      doctorId: ['', Validators.required],
      startDate: [null, Validators.required],
      endDate: [null, Validators.required],
      shifts: [[], Validators.required],
      excludeHolidays: [true]
    });
  }

  ngOnInit() {
    this.loadDoctors();
    this.loadShifts();
  }

  loadDoctors() {
    this.doctors = [
      { id: 1, name: 'دکتر علی احمدی' },
      { id: 2, name: 'دکتر مریم رضایی' }
    ];
  }

  loadShifts() {
    this.shifts = [
      { id: 1, name: 'صبح' },
      { id: 2, name: 'عصر' }
    ];
  }

  toggleShift(shiftId: number) {
    const shifts = this.generateForm.get('shifts')?.value || [];
    const index = shifts.indexOf(shiftId);
    if (index > -1) {
      shifts.splice(index, 1);
    } else {
      shifts.push(shiftId);
    }
    this.generateForm.patchValue({ shifts });
  }

  isShiftSelected(shiftId: number): boolean {
    const shifts = this.generateForm.get('shifts')?.value || [];
    return shifts.includes(shiftId);
  }


  generateSchedule() {
    if (this.generateForm.valid) {
      this.isGenerating = true;
      // در اینجا باید با API ارتباط برقرار شود
      setTimeout(() => {
        this.isGenerating = false;
        this.snackbarService.success('زمان‌بندی با موفقیت تولید شد', 'بستن', 3000);
        this.generateForm.reset({ excludeHolidays: true });
      }, 2000);
    }
  }
}
