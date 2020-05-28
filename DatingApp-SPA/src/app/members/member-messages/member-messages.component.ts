import { Component, OnInit, Input } from '@angular/core';
import { Message } from '../../_models/message';
import { UserService } from '../../_services/user.service';
import { AuthService } from '../../_services/auth.service';
import { AlertifyService } from '../../_services/alertify.service';
import { tap } from 'rxjs/operators';

@Component({
  selector: 'app-member-messages',
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.css']
})
export class MemberMessagesComponent implements OnInit {
  @Input() recipientId: number;
  messages: Message[];
  newMessage: any = {};

  constructor(private userService: UserService, private authService: AuthService,
              private alertify: AlertifyService) { }

  ngOnInit() {
    this.loadMessages();
  }

  loadMessages() {
    const currentUserId = +this.authService.decodedToken.nameid;
    this.userService.getMessageThread(this.authService.decodedToken.nameid, this.recipientId)
      //  is read
      .pipe(
        tap(messages => {
          for (let i = 0; i < messages.length; i++) {
            if (messages[i].isRead === false && messages[i].recipientId === currentUserId) {
              this.userService.markAsRead(currentUserId, messages[i].id);
            }
          }
        })
      )
      .subscribe(messages => {
        this.messages = messages;
    }, error => {
      this.alertify.error(error);
    });
  }

  sendMessage() {
    this.newMessage.recipientId = this.recipientId;
    this.userService.sendMessage(this.authService.decodedToken.nameid, this.newMessage)
      .subscribe((message: Message) => {
        //  adding a message to the aray but not with push because message shoudl be on start not at the end of the array
        this.messages.unshift(message);
        //  after adding it to aray we reset the form
        this.newMessage.content = '';
    }, error => {
      this.alertify.error(error);
    });
  }

}
