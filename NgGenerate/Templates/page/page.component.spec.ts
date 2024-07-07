import { ComponentFixture, TestBed } from '@angular/core/testing';

import { %NAME%Component } from './%name%.component';

describe('%NAME%Component', () => {
  let component: %NAME%Component;
  let fixture: ComponentFixture<%NAME%Component>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [%NAME%Component]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(%NAME%Component);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
