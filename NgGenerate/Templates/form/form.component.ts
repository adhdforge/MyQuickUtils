import { Component } from '@angular/core';
import { DynamicComponent } from '../dynamic/dynamic.component';

@Component({
  selector: 'app-%name%',
  templateUrl: '../dynamic/dynamic.component.html',
  styleUrl: '../dynamic/dynamic.component.scss'
})
export class %NAME%Component extends DynamicComponent {
  override async OnInit() {
    this.pageId = '0';
    this.state.pageNavButton.next('home');
  }
}
