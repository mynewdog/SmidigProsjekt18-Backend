import React from 'react';
import PropTypes from 'prop-types';
import Paper from '@material-ui/core/Paper';
import Button from '@material-ui/core/Button';
import TextField from '@material-ui/core/TextField';
import { withStyles } from '@material-ui/core/styles';
import ThemeDefault from '../styles/theme-default';
import { MuiThemeProvider } from '@material-ui/core/styles';
import auth from '../auth';
import CssBaseline from '@material-ui/core/CssBaseline';
import classNames from 'classnames';

const styles = theme => ({
    loginContainer: {
        minWidth: 420,
        maxWidth: 500,
        height: 'auto',
        position: 'absolute',
        top: '25%',
        left: 0,
        right: 0,
        margin: 'auto'
    },
    paper: {
        padding: 20,
        overflow: 'auto'
    },
    Button: {
        float: 'right'
    },
    logo: {
        textAlign: "center",
        width: 16,
        height: 16,
        borderRadius: 16,
        backgroundColor: '#92d050',
        marginRight: 6,
        display: 'inline-block'
    },
    text: {
        color: '#333',
        fontWeight: 'bold',
        backgroundColor: 'transparent',
        verticalAlign: 'text-bottom'
    },
    error: { color: 'red' },
    signintext: {
        font: 'Segoe UI',
        fontSize: 25,
        marginBottom: 20
    },
    centerlogo: {
        textAlign: "center",
        marginTop: 40,
        marginBottom: 40
    },
    centertext: {
        textAlign: "center",
        marginTop: 10,
        marginBottom: 10
    }
});


class LoginPage extends React.Component {
    constructor(props) {
        super(props);
        this.state = { user: '', password: '' };
    }

    render() {
        let { user, password, error } = this.state;
        let { onAuthenticated, classes } = this.props;

        const handleLogin = _ => {
          this.setState({ error: null });
            auth.signIn(user, password)
            .then(_ => onAuthenticated())
            .catch(error => {
                if (error.message === '400') this.setState({ error: 'Invalid password' });
                else this.setState({ error: error.message });
            });
        };

        return (
            <MuiThemeProvider theme={ThemeDefault}>
                <CssBaseline />
                <div>
                    <div className={classes.loginContainer}>
                        <Paper className={classes.paper}>
                            <div className={classes.centerlogo}>
                                <span className={classes.logo} />
                                <span className={classes.text}>Tjommis Management</span>
                            </div>
                            <div className={classes.centertext}>
                                <span className={classes.signintext}>Sign In</span>
                            </div>
                            <div className={classes.centertext}>
                                <span className={classes.signintent}>To continue to your page</span>
                            </div>
                            <form>
                                <TextField fullWidth={true}
                                    type="email"
                                    autoComplete="email"
                                    margin="normal"
                                    variant="outlined"
                                    label="Username or Email"
                                    value={user}
                                    onChange={event => this.setState({ user: event.target.value })}
                                />
                                <TextField
                                    fullWidth={true}
                                    label="Type in your password"
                                    type="password"
                                    variant="outlined"
                                    value={password}
                                    onChange={event => this.setState({ password: event.target.value })}
                                />
                                {error ? <div className={classes.error}>{error}</div> : null}
                                <div style={{ marginTop: 15 }}>
                                    <span>
                                        <Button variant="contained"
                                            label="Login"
                                            onClick={handleLogin}
                                            className={classes.Button}
                                            color="primary"
                                        >
                                            Next
                                        </Button>
                                    </span>
                                </div>
                            </form>
                        </Paper>
                    </div>
                </div>
            </MuiThemeProvider>
        );
    }
}

LoginPage.propTypes = {
    onAuthenticated: PropTypes.func
};

export default withStyles(styles)(LoginPage);
