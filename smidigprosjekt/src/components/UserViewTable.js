import React from 'react';
import { Table,TableHead,TableRow,TableCell, TableBody} from "@material-ui/core";
import { grey } from "@material-ui/core/colors";

const styles = {
    addButton: { margin: '1em' },
    removeIcon: { fill: grey },
    columns: {
        connected: { width: '25%' },
        username: { width: '25%' },
        studie: { width: '25%' },
        intitutt: { width: '25%' }
    },
    pagination: { marginTop: '1em' }
};

export const UserViewTable = props => { 
        return (
            <Table>
                <TableHead>
                    <TableRow>
                        <TableCell style={styles.columns.connected}>Tilkoblet</TableCell>
                        <TableCell style={styles.columns.username}>Brukernavn</TableCell>
                        <TableCell style={styles.columns.studie}>Studie</TableCell>
                        <TableCell style={styles.columns.institutt}>Institutt</TableCell>
                    </TableRow>
                </TableHead>
                <TableBody>
                    {props.users.map(item => {
                        return <TableRow key={item.Username}>
                            <TableCell style={styles.columns.connected}>{item.Connected ? "Yes" : "No"}</TableCell>
                            <TableCell style={styles.columns.username}>{item.Username}</TableCell>
                            <TableCell style={styles.columns.studie}>{item.Studie}</TableCell>
                            <TableCell style={styles.columns.institutt}>{item.Institutt}</TableCell>
                        </TableRow>
                        }
                    )}
                </TableBody>
            </Table>)    
}