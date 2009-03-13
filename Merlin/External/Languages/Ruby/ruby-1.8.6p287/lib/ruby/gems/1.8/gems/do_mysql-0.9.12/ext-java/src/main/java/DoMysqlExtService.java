import data_objects.drivers.DriverDefinition;
import do_mysql.MySqlDriverDefinition;

public class DoMysqlExtService extends AbstractDataObjectsExtService {

    private final static DriverDefinition driver = new MySqlDriverDefinition();
    public final static String RUBY_MODULE_NAME = "Mysql";
    public final static String RUBY_ERROR_NAME = "MysqlError";

    public String getModuleName() {
        return RUBY_MODULE_NAME;
    }

    @Override
    public String getErrorName() {
        return RUBY_ERROR_NAME;
    }

    public DriverDefinition getDriverDefinition() {
        return driver;
    }

}
