import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MazeManagerComponent } from './maze-manager.component';

describe('MazeManagerComponent', () => {
  let component: MazeManagerComponent;
  let fixture: ComponentFixture<MazeManagerComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ MazeManagerComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(MazeManagerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
