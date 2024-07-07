import { Component } from '@angular/core';
import { PageBase } from '../page-base';

@Component({
  selector: 'app-%name%',
  templateUrl: './%name%.component.html',
  styleUrl: './%name%.component.scss'
})
export class %NAME%Component extends PageBase {
  override async OnInit() {
    this.state.pageNavButton.next('home');
  }
}
