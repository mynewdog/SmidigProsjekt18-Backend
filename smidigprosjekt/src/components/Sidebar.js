import React from 'react';
import PropTypes from 'prop-types';
import { RouteLink } from 'dotnetify';
import { withStyles } from '@material-ui/core/styles';
import classNames from 'classnames';
import Drawer from '@material-ui/core/Drawer';
import Divider from '@material-ui/core/Divider';
import List from '@material-ui/core/List';
import ListItem from '@material-ui/core/ListItem';
import ListItemText from '@material-ui/core/ListItemText';
import Icon from '@material-ui/core/Icon';

const drawerWidth = 200;
const styles = theme => ({
  drawer: {
    width: drawerWidth,
    flexShrink: 0,
    whiteSpace: 'nowrap',
  },
  drawerOpen: {
    width: drawerWidth,
    transition: theme.transitions.create('width', {
      easing: theme.transitions.easing.sharp,
      duration: theme.transitions.duration.enteringScreen,
    }),
  },
  drawerClose: {
    transition: theme.transitions.create('width', {
      easing: theme.transitions.easing.sharp,
      duration: theme.transitions.duration.leavingScreen,
    }),
    overflowX: 'hidden',
    width: theme.spacing.unit * 7 + 1,
    [theme.breakpoints.up('sm')]: {
      width: theme.spacing.unit * 9 + 1,
    },
  },
  drawerPaper: {
    width: drawerWidth,
  },
  toolbar: {
    marginTop: theme.mixins.toolbar.minHeight
  },
  content: {
    flexGrow: 1,
    marginTop: theme.mixins.toolbar.minHeight,
    padding: theme.spacing.unit,
  },
});

const Sidebar = props => {
  const { vm, menus, classes,open } = props;
  return (
    <Drawer
      className={classNames(classes.drawer, {
      [classes.drawerOpen]: open,
      [classes.drawerClose]: !open,
        })}
      classes={{
        paper: classNames({
          [classes.drawerOpen]: open,
          [classes.drawerClose]: !open,
        }),
      }}
      open={open}
      variant="permanent"
      className={classes.drawer}
      classes={{
        paper: classes.drawerPaper,
      }}>
      <div className={classes.toolbar}>
        <List>
          {menus.map((menu, index) => (
            <RouteLink key={index} vm={vm} route={menu.Route} >
              <ListItem button>
                <Icon className="material-icons">{menu.Icon}</Icon>
                <ListItemText>{menu.Title}</ListItemText>
              </ListItem>
            </RouteLink>
          ))}
        </List>
      </div>
    </Drawer>
  );
};

Sidebar.propTypes = {
  menus: PropTypes.array,
  username: PropTypes.string,
  userAvatarUrl: PropTypes.string,
  theme: PropTypes.object.isRequired,
  classes: PropTypes.object.isRequired,
  open: PropTypes.bool,
};

export default withStyles(styles)(Sidebar);