import React from 'react';
import dotnetify from 'dotnetify';
import PropTypes from 'prop-types';
import { withStyles } from '@material-ui/core/styles';
import { MuiThemeProvider } from '@material-ui/core/styles';
import FloatingActionButton from '@material-ui/core/Fab';
import Snackbar from '@material-ui/core/Snackbar';
import TextField from '@material-ui/core/TextField';
//import { Table, TableBody, TableHeader, TableCell, TableRow, TableCell } from '@material-ui/core/Table';
import Table from '@material-ui/core/Table';
import TableBody from '@material-ui/core/TableBody';
import TableCell from '@material-ui/core/TableCell';
import TableHeader from '@material-ui/core/TableHead';
import TableRow from '@material-ui/core/TableRow';

import IconRemove from '@material-ui/icons/Remove';
import ContentAdd from '@material-ui/icons/Add';
import { pink500, grey200, grey500 } from '@material-ui/core/colors';
import BasePage from '../components/BasePage';
import Pagination from '../components/table/Pagination';
import InlineEdit from '../components/table/InlineEdit';
import ThemeDefault from '../styles/theme-default';

const styles = theme => ({
});
class InterestPage extends React.Component {
    constructor(props) {
        super(props);
        this.vm = dotnetify.react.connect('ManageInterestVM', this);
        this.state = { Interests: [], addName: '', Pages: [] };
        this.dispatch = state => this.vm.$dispatch(state); 
        /*
         * From
            public class InterestItem
            {
                public int Id { get; set; }
                public string Key { get; set; }
                public string Name { get; set; }
                public string Category { get; set; }
            }
         * */
    }

    componentWillUnmount() {
        this.vm.$destroy();
    }

    render() {
        let { Interests, addName, addCategory, Pages, SelectedPage, ShowNotification } = this.state;

        const styles = {
            addButton: { margin: '1em' },
            removeIcon: { fill: grey500 },
            columns: {
                category: { width: '40%' },
                tag: { width: '40%' },
                remove: { width: '20%' }
            },
            pagination: { marginTop: '1em' }
        };

        const handleAdd = _ => {
            if (addName) {
                console.log("adding");
                this.dispatch({ Add: { Name: addName, Category: addCategory } });
                this.setState({ addName: '', addCategory: '' });
            }
        };

        const handleUpdate = interest => {
            let newState = Interests.map(item => (item.Key === interest.Key ? Object.assign(item, interest) : item));
            this.setState({ Interests: newState });
            this.dispatch({ Update: interest });
        };

        const handleSelectPage = page => {
            const newState = { SelectedPage: page };
            this.setState(newState);
            this.dispatch(newState);
        };

        const hideNotification = _ => this.setState({ ShowNotification: false });

        return (
            <MuiThemeProvider theme={ThemeDefault}>
                <BasePage title="Interests" navigation="Application / Table Page">
                    <div>
                        <div>

                            <TextField
                                id="AddCategory"
                                label="Category"
                                value={addCategory}
                                onKeyPress={event => (event.key === 'Enter' ? handleAdd() : null)}
                                onChange={event => this.setState({ addCategory: event.target.value })}
                            />
                            <TextField
                                id="AddName"
                                label="TagName"
                                value={addName}
                                onKeyPress={event => (event.key === 'Enter' ? handleAdd() : null)}
                                onChange={event => this.setState({ addName: event.target.value })}
                            />

                            <FloatingActionButton onClick={handleAdd} style={styles.addButton} size="small" >
                                <ContentAdd />
                            </FloatingActionButton>
                        </div>

                        <Table>
                            <TableHeader>
                                <TableRow>
                                    <TableCell style={styles.columns.category}>Category</TableCell>
                                    <TableCell style={styles.columns.tag}>Tag</TableCell>
                                    <TableCell style={styles.columns.remove}>Remove</TableCell>
                                </TableRow>
                            </TableHeader>
                            <TableBody>
                                {Interests.map(item => (
                                    <TableRow key={item.Id}>
                                        <TableCell style={styles.columns.category}>
                                            <InlineEdit onChange={value => handleUpdate({ Id: item.Key, FirstName: value })}>{item.Category}</InlineEdit>
                                        </TableCell>
                                        <TableCell style={styles.columns.tag}>
                                            <InlineEdit onChange={value => handleUpdate({ Id: item.Key, LastName: value })}>{item.Name}</InlineEdit>
                                        </TableCell>
                                        <TableCell style={styles.columns.remove}>
                                            <FloatingActionButton
                                                size="small"
                                                onClick={_ => this.dispatch({ Delete: item.Key })}
                                                color="primary"
                                            >
                                                <IconRemove />
                                            </FloatingActionButton>
                                        </TableCell>
                                    </TableRow>
                                ))}
                            </TableBody>
                        </Table>

                        <Pagination style={styles.pagination} pages={Pages} select={SelectedPage} onSelect={handleSelectPage} />

                        <Snackbar open={ShowNotification} message="Changes saved" autoHideDuration={1000} onRequestClose={hideNotification} />
                    </div>
                </BasePage>
            </MuiThemeProvider>
        );
    };
};

export default withStyles(styles)(InterestPage);