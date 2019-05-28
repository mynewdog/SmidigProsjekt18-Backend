class SmidigProsjekt extends React.Component {
    constructor(props) {
        super(props);
        dotnetify.react.connect('FrontEndManagementVM', this);
        this.state = { Greetings: '', ServerTime: '' };
    }

    render() {
        return (
            <div>
                <p>{this.state.Greetings}</p>
                <p>Server time is: {this.state.ServerTime}</p>
            </div>
        );
    }
}

ReactDOM.render(<SmidigProsjekt />, document.getElementById('App'));