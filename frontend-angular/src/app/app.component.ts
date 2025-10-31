import { Component } from '@angular/core';
import { LastThreeComponent } from './last-three/last-three.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [LastThreeComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  title = 'frontend-angular';
}
