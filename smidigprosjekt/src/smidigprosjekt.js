import React from 'react';
import dotnetify from 'dotnetify';

export default class SmidigProsjekt extends React.Component {
    constructor(props) {
        super(props);
        dotnetify.react.connect('FrontEndManagementVM', this);
        this.state = { Greetings: '', ServerTime: '' };
    }

    render() {
        return (
            <div>
                <h4>SmidigProsjekt</h4>
                <p>{this.state.Greetings}</p>
                <p>Server time is: {this.state.ServerTime}</p>
            </div>
        );
    }
};