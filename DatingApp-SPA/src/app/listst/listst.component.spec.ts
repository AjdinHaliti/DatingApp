/* tslint:disable:no-unused-variable */
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { DebugElement } from '@angular/core';

import { ListstComponent } from './listst.component';

describe('ListstComponent', () => {
  let component: ListstComponent;
  let fixture: ComponentFixture<ListstComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ListstComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ListstComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
